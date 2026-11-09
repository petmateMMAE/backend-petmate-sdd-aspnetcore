using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Users;

namespace PetMate.Application.Users;

/// <summary>Perfil propio (pm-037/pm-038). Nunca incluye el hash de la contraseña. <c>ToString</c> no imprime el correo.</summary>
public sealed record UserProfileResult(Guid Id, string Email, UserRole Role, string? DisplayName, DateTime CreatedAtUtc, string? Phone = null)
{
    public override string ToString() => nameof(UserProfileResult);

    internal static UserProfileResult From(User user)
        => new(user.Id, user.Email.Value, user.Role, user.DisplayName, user.CreatedAtUtc, user.Phone);
}

/// <summary>pm-037: perfil del usuario autenticado. El identificador sale solo del Access Token validado.</summary>
public sealed record GetMyProfileQuery(Guid UserId) : IQuery<UserProfileResult?>;

/// <summary>pm-038, pm-091: actualiza los campos editables del perfil propio (nombre visible y celular opcional).</summary>
public sealed record UpdateMyProfileCommand(Guid UserId, string DisplayName, string? Phone = null) : ICommand<UserProfileResult?>
{
    public override string ToString() => nameof(UpdateMyProfileCommand);
}

/// <summary>Devuelve <c>null</c> si la cuenta del token ya no existe.</summary>
public sealed class GetMyProfileHandler(IUserRepository users) : IRequestHandler<GetMyProfileQuery, UserProfileResult?>
{
    public async Task<UserProfileResult?> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await users.FindById(request.UserId, cancellationToken).ConfigureAwait(false);
        return user is null ? null : UserProfileResult.From(user);
    }
}

/// <summary>Devuelve <c>null</c> si la cuenta del token ya no existe; si no, el perfil actualizado.</summary>
public sealed class UpdateMyProfileHandler(IUserRepository users, TimeProvider timeProvider)
    : IRequestHandler<UpdateMyProfileCommand, UserProfileResult?>
{
    public async Task<UserProfileResult?> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await users.FindById(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        user.ChangeDisplayName(request.DisplayName, now);
        // Reemplazo completo (pm-091): un celular ausente, nulo o vacío lo quita.
        user.ChangePhone(request.Phone, now);
        await users.UpdateProfile(user, cancellationToken).ConfigureAwait(false);
        return UserProfileResult.From(user);
    }
}

/// <summary>Nombre obligatorio, de 1 a 80 caracteres tras recortar espacios y sin caracteres de control.</summary>
public sealed class UpdateMyProfileCommandValidator : IRequestValidator<UpdateMyProfileCommand>
{
    private static readonly Task<IReadOnlyList<ValidationFailure>> NoFailures = Task.FromResult<IReadOnlyList<ValidationFailure>>([]);

    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (!User.IsValidDisplayName(request.DisplayName))
        {
            var message = string.IsNullOrWhiteSpace(request.DisplayName)
                ? "El nombre es obligatorio."
                : $"El nombre debe tener entre 1 y {User.MaxDisplayNameLength} caracteres y no contener caracteres de control.";
            failures.Add(new ValidationFailure("displayName", message));
        }

        if (!User.TryNormalizePhone(request.Phone, out _))
        {
            failures.Add(new ValidationFailure("phone", $"El celular debe tener {User.PhoneDigits} dígitos (se admite el prefijo +52, espacios y guiones)."));
        }

        return failures.Count == 0 ? NoFailures : Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}
