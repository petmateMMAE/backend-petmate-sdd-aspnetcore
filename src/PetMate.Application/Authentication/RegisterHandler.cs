using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>Alta pública: siempre crea un Owner. El hash se calcula antes de consultar la unicidad para no variar el tiempo según el correo.</summary>
public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    AuthTokenIssuer issuer,
    TimeProvider timeProvider) : IRequestHandler<RegisterCommand, AuthResult<AuthTokensResult>>
{
    public async Task<AuthResult<AuthTokensResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Email.Create(request.Email);
        var passwordHash = PasswordHash.Create(hasher.Hash(request.Password));
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = User.RegisterOwner(email, passwordHash, now);

        // La unicidad la decide el índice único de Atlas; no hay lectura previa ni condición de carrera.
        if (!await users.TryAdd(user, cancellationToken).ConfigureAwait(false))
        {
            return AuthResult.Failure<AuthTokensResult>(AuthError.EmailAlreadyRegistered);
        }

        return AuthResult.Success(await issuer.StartSession(user, now, cancellationToken).ConfigureAwait(false));
    }
}
