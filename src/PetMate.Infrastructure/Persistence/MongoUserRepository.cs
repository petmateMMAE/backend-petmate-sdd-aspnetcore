using MongoDB.Driver;
using PetMate.Application.Persistence;
using PetMate.Domain.Common;
using PetMate.Domain.Users;

namespace PetMate.Infrastructure.Persistence;

public sealed class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> users;

    public MongoUserRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        users = database.GetCollection<UserDocument>(names.Users);
    }

    public async Task<bool> TryAdd(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            await users.InsertOneAsync(MongoMapping.ToDocument(user), cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // El índice único de email decide la carrera; no hay lectura previa.
            return false;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<User?> FindByEmail(Email email, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(email);
        return await FindOne(Builders<UserDocument>.Filter.Eq(document => document.Email, email.Value), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<User?> FindById(Guid id, CancellationToken cancellationToken)
        => await FindOne(Builders<UserDocument>.Filter.Eq(document => document.Id, id), cancellationToken).ConfigureAwait(false);

    public async Task UpdatePasswordHash(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            await users.UpdateOneAsync(
                Builders<UserDocument>.Filter.Eq(document => document.Id, user.Id),
                Builders<UserDocument>.Update
                    .Set(document => document.PasswordHash, user.PasswordHash.Value)
                    .Set(document => document.UpdatedAtUtc, user.UpdatedAtUtc),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task UpdateProfile(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            await users.UpdateOneAsync(
                Builders<UserDocument>.Filter.Eq(document => document.Id, user.Id),
                Builders<UserDocument>.Update
                    .Set(document => document.DisplayName, user.DisplayName)
                    .Set(document => document.UpdatedAtUtc, user.UpdatedAtUtc),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    private async Task<User?> FindOne(FilterDefinition<UserDocument> filter, CancellationToken cancellationToken)
    {
        try
        {
            var document = await users.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : MongoMapping.ToDomain(document);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}
