using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>
/// Login. Correo inexistente y contraseña incorrecta devuelven el mismo error y consumen un Argon2id (contra un hash
/// ficticio calculado con los parámetros vigentes) para no filtrar la existencia de la cuenta por tiempo.
/// El modelo de usuario aprobado no tiene estado de cuenta: no hay comprobación de activación.
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResult<AuthTokensResult>>
{
    private readonly IUserRepository users;
    private readonly IPasswordHasher hasher;
    private readonly AuthTokenIssuer issuer;
    private readonly TimeProvider timeProvider;
    private readonly string dummyHash;

    public LoginHandler(IUserRepository users, IPasswordHasher hasher, AuthTokenIssuer issuer, TimeProvider timeProvider)
    {
        this.users = users;
        this.hasher = hasher;
        this.issuer = issuer;
        this.timeProvider = timeProvider;
        dummyHash = hasher.Hash(Guid.NewGuid().ToString("N") + "-sin-cuenta");
    }

    public async Task<AuthResult<AuthTokensResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Email.Create(request.Email);
        var user = await users.FindByEmail(email, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            _ = hasher.Verify(request.Password, dummyHash);
            return AuthResult.Failure<AuthTokensResult>(AuthError.InvalidCredentials);
        }

        var verification = hasher.Verify(request.Password, user.PasswordHash.Value);
        if (verification == PasswordVerificationResult.Failed)
        {
            return AuthResult.Failure<AuthTokensResult>(AuthError.InvalidCredentials);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.ChangePasswordHash(PasswordHash.Create(hasher.Hash(request.Password)), now);
            await users.UpdatePasswordHash(user, cancellationToken).ConfigureAwait(false);
        }

        return AuthResult.Success(await issuer.StartSession(user, now, cancellationToken).ConfigureAwait(false));
    }
}
