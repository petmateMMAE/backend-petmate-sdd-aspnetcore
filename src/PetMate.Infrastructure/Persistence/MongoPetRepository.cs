using MongoDB.Driver;
using PetMate.Application.Persistence;
using PetMate.Domain.Common;
using PetMate.Domain.Pets;

namespace PetMate.Infrastructure.Persistence;

/// <summary>
/// Mascotas en Atlas (pm-039). Cada filtro incluye el propietario: una mascota ajena se comporta como inexistente y no se
/// puede leer, cambiar ni borrar con su identificador. El borrado es físico.
/// </summary>
public sealed class MongoPetRepository : IPetRepository
{
    private readonly IMongoCollection<PetDocument> pets;

    public MongoPetRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        pets = database.GetCollection<PetDocument>(names.Pets);
    }

    public async Task Add(Pet pet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pet);
        await Execute(() => pets.InsertOneAsync(MongoMapping.ToDocument(pet), cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Pet?> FindOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken)
    {
        try
        {
            var document = await pets.Find(Owned(petId, ownerId)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : MongoMapping.ToDomain(await CompleteQr(document, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<IReadOnlyList<Pet>> ListByOwner(Guid ownerId, int skip, int take, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        try
        {
            var documents = await pets.Find(Builders<PetDocument>.Filter.Eq(document => document.OwnerId, ownerId))
                .SortByDescending(document => document.CreatedAtUtc)
                .ThenByDescending(document => document.Id)
                .Skip(skip)
                .Limit(take)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<Pet>(documents.Count);
            foreach (var document in documents)
            {
                result.Add(MongoMapping.ToDomain(await CompleteQr(document, cancellationToken).ConfigureAwait(false)));
            }

            return result;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<long> CountByOwner(Guid ownerId, CancellationToken cancellationToken)
        => await Execute(() => pets.CountDocumentsAsync(
            Builders<PetDocument>.Filter.Eq(document => document.OwnerId, ownerId), cancellationToken: cancellationToken)).ConfigureAwait(false);

    public async Task<bool> UpdateOwned(Pet pet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pet);
        var document = MongoMapping.ToDocument(pet);
        var update = Builders<PetDocument>.Update
            .Set(item => item.Name, document.Name)
            .Set(item => item.Species, document.Species)
            .Set(item => item.Breed, document.Breed)
            .Set(item => item.BirthDate, document.BirthDate)
            .Set(item => item.Sex, document.Sex)
            .Set(item => item.PhotoUrl, document.PhotoUrl)
            .Set(item => item.UpdatedAtUtc, document.UpdatedAtUtc);

        var result = await Execute(() => pets.UpdateOneAsync(Owned(pet.Id, pet.OwnerId), update, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    public async Task<bool> ReplaceQrOwned(Pet pet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pet);
        var update = Builders<PetDocument>.Update
            .Set(item => item.QrToken, pet.Qr.Token)
            .Set(item => item.QrCode, pet.Qr.Code)
            .Set(item => item.UpdatedAtUtc, pet.UpdatedAtUtc);
        var result = await Execute(() => pets.UpdateOneAsync(Owned(pet.Id, pet.OwnerId), update, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    public async Task<Pet?> FindByQr(string tokenOrCode, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenOrCode);
        var builder = Builders<PetDocument>.Filter;
        var filter = tokenOrCode.Length == PetQr.TokenLength
            ? builder.Eq(document => document.QrToken, tokenOrCode)
            : tokenOrCode.Length == PetQr.CodeLength
                ? builder.Eq(document => document.QrCode, tokenOrCode.ToUpperInvariant())
                : null;
        if (filter is null)
        {
            return null;
        }

        try
        {
            var document = await pets.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : MongoMapping.ToDomain(document);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<bool> DeleteOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await Execute(() => pets.DeleteOneAsync(Owned(petId, ownerId), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount == 1;
    }

    // Mascotas creadas antes del QR (pm-093): se les asigna uno al leerlas. El filtro por qrToken nulo evita que dos lecturas
    // simultáneas pisen el valor; si otra lectura ganó, se usa el que ya quedó guardado.
    private async Task<PetDocument> CompleteQr(PetDocument document, CancellationToken cancellationToken)
    {
        if (PetQr.IsWellFormed(document.QrToken, document.QrCode))
        {
            return document;
        }

        var qr = PetQr.Generate();
        var filter = Builders<PetDocument>.Filter.And(
            Builders<PetDocument>.Filter.Eq(item => item.Id, document.Id),
            Builders<PetDocument>.Filter.Eq(item => item.QrToken, null));
        var update = Builders<PetDocument>.Update.Set(item => item.QrToken, qr.Token).Set(item => item.QrCode, qr.Code);
        var result = await pets.UpdateOneAsync(filter, update, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result.ModifiedCount == 1)
        {
            document.QrToken = qr.Token;
            document.QrCode = qr.Code;
            return document;
        }

        return await pets.Find(Builders<PetDocument>.Filter.Eq(item => item.Id, document.Id)).FirstAsync(cancellationToken).ConfigureAwait(false);
    }

    private static FilterDefinition<PetDocument> Owned(Guid petId, Guid ownerId)
        => Builders<PetDocument>.Filter.And(
            Builders<PetDocument>.Filter.Eq(document => document.Id, petId),
            Builders<PetDocument>.Filter.Eq(document => document.OwnerId, ownerId));

    private static async Task Execute(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    private static async Task<T> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}
