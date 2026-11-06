using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Users;

namespace PetMate.Application.Users;

/// <summary>Perfil propio (pm-037/pm-038). Nunca incluye el hash de la contraseña. <c>ToString</c> no imprime el correo.</summary>
public sealed record UserProfileResult(Guid Id, string Email, UserRole Role, string? DisplayName, DateTime CreatedAtUtc)
{
    public override string ToString() => nameof(UserProfileResult);

    internal static UserProfileResult From(User user)
        => new(user.Id, user.Email.Value, user.Role, user.DisplayName, user.CreatedAtUtc);
}

/// <summary>pm-037: perfil del usuario autenticado. El identificador sale solo del Access Token validado.</summary>
public sealed record GetMyProfileQuery(Guid UserId) : IQuery<UserProfileResult?>;

/// <summary>pm-038: actualiza los campos editables del perfil propio (por ahora, el nombre visible).</summary>
public sealed record UpdateMyProfileCommand(Guid UserId, string DisplayName) : ICommand<UserProfileResult?>
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

        user.ChangeDisplayName(request.DisplayName, timeProvider.GetUtcNow().UtcDateTime);
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
        if (User.IsValidDisplayName(request.DisplayName))
        {
            return NoFailures;
        }

        var message = string.IsNullOrWhiteSpace(request.DisplayName)
            ? "El nombre es obligatorio."
            : $"El nombre debe tener entre 1 y {User.MaxDisplayNameLength} caracteres y no contener caracteres de control.";
        return Task.FromResult<IReadOnlyList<ValidationFailure>>([new ValidationFailure("displayName", message)]);
    }
}
