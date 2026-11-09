using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>Alta pública: siempre crea un User. El hash se calcula antes de consultar la unicidad para no variar el tiempo según el correo.</summary>
public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    AuthTokenIssuer issuer,
    TimeProvider timeProvider,
    IPasswordHashingLimiter? hashingLimiter = null) : IRequestHandler<RegisterCommand, AuthResult<AuthTokensResult>>
{
    public async Task<AuthResult<AuthTokensResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Email.Create(request.Email);
        string rawHash;
        if (hashingLimiter is null)
        {
            rawHash = hasher.Hash(request.Password);
        }
        else
        {
            // Limitador de hashing (pm-034 CA-03): si hay demasiado trabajo en cola se rechaza con 429 en vez de saturar la CPU.
            var hashed = await hashingLimiter.Run(() => hasher.Hash(request.Password), cancellationToken).ConfigureAwait(false);
            if (!hashed.Accepted)
            {
                return AuthResult.Throttled<AuthTokensResult>(TimeSpan.FromSeconds(1));
            }

            rawHash = hashed.Value!;
        }

        var passwordHash = PasswordHash.Create(rawHash);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = User.Register(email, passwordHash, request.DisplayName, now);

        // La unicidad la decide el índice único de Atlas; no hay lectura previa ni condición de carrera.
        if (!await users.TryAdd(user, cancellationToken).ConfigureAwait(false))
        {
            return AuthResult.Failure<AuthTokensResult>(AuthError.EmailAlreadyRegistered);
        }

        return AuthResult.Success(await issuer.StartSession(user, now, cancellationToken).ConfigureAwait(false));
    }
}
