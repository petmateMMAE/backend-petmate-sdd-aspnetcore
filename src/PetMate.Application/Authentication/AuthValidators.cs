using PetMate.Application.Validation;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

internal static class AuthValidation
{
    public const int MinPasswordLength = 12;
    public const int MaxPasswordLength = 128;

    private static readonly Task<IReadOnlyList<ValidationFailure>> NoFailures = Task.FromResult<IReadOnlyList<ValidationFailure>>([]);

    public static Task<IReadOnlyList<ValidationFailure>> Result(List<ValidationFailure> failures)
        => failures.Count == 0 ? NoFailures : Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);

    public static void CheckEmail(string? email, List<ValidationFailure> failures)
    {
        if (!Email.TryCreate(email, out _))
        {
            failures.Add(new ValidationFailure("email", "El correo electrónico no es válido."));
        }
    }

    public static void CheckRefreshToken(string? token, List<ValidationFailure> failures)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            failures.Add(new ValidationFailure("refreshToken", "El refresh token es obligatorio."));
        }
    }
}

/// <summary>Política de contraseña de Register: 12–128 caracteres, sin reglas de composición (guía NIST 800-63B).</summary>
public sealed class RegisterCommandValidator : IRequestValidator<RegisterCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(RegisterCommand request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        AuthValidation.CheckEmail(request.Email, failures);

        if (!User.IsValidDisplayName(request.DisplayName))
        {
            failures.Add(new ValidationFailure("displayName", $"El nombre es obligatorio y debe tener de 1 a {User.MaxDisplayNameLength} caracteres."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            failures.Add(new ValidationFailure("password", "La contraseña es obligatoria."));
        }
        else if (request.Password.Length < AuthValidation.MinPasswordLength)
        {
            failures.Add(new ValidationFailure("password", $"La contraseña debe tener al menos {AuthValidation.MinPasswordLength} caracteres."));
        }
        else if (request.Password.Length > AuthValidation.MaxPasswordLength)
        {
            failures.Add(new ValidationFailure("password", $"La contraseña no debe exceder {AuthValidation.MaxPasswordLength} caracteres."));
        }

        return AuthValidation.Result(failures);
    }
}

public sealed class LoginCommandValidator : IRequestValidator<LoginCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(LoginCommand request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        AuthValidation.CheckEmail(request.Email, failures);

        // No se aplica la política de longitud mínima: no debe revelar nada sobre contraseñas existentes.
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            failures.Add(new ValidationFailure("password", "La contraseña es obligatoria."));
        }
        else if (request.Password.Length > AuthValidation.MaxPasswordLength)
        {
            failures.Add(new ValidationFailure("password", $"La contraseña no debe exceder {AuthValidation.MaxPasswordLength} caracteres."));
        }

        return AuthValidation.Result(failures);
    }
}

public sealed class RefreshCommandValidator : IRequestValidator<RefreshCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(RefreshCommand request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        AuthValidation.CheckRefreshToken(request.RefreshToken, failures);
        return AuthValidation.Result(failures);
    }
}

public sealed class LogoutCommandValidator : IRequestValidator<LogoutCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(LogoutCommand request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        AuthValidation.CheckRefreshToken(request.RefreshToken, failures);
        return AuthValidation.Result(failures);
    }
}
