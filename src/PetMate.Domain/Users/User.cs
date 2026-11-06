using PetMate.Domain.Common;

namespace PetMate.Domain.Users;

/// <summary>Cuenta de usuario. El rol no puede elegirse desde fuera: el alta pública crea siempre un Owner.</summary>
public sealed class User
{
    /// <summary>Longitud máxima del nombre visible (pm-038).</summary>
    public const int MaxDisplayNameLength = 80;

    private User(Guid id, Email email, PasswordHash passwordHash, UserRole role, DateTime createdAtUtc, DateTime updatedAtUtc, string? displayName)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        DisplayName = displayName;
    }

    public Guid Id { get; }

    public Email Email { get; }

    public PasswordHash PasswordHash { get; private set; }

    public UserRole Role { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>Nombre con el que se muestra al usuario. Opcional: el alta solo pide correo y contraseña.</summary>
    public string? DisplayName { get; private set; }

    /// <summary>Única fábrica de alta: crea un Owner. Business se define en un slice posterior.</summary>
    public static User RegisterOwner(Email email, PasswordHash passwordHash, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        return new User(Guid.CreateVersion7(), email, passwordHash, UserRole.Owner, now, now, displayName: null);
    }

    /// <summary>Rehidrata un usuario ya persistido. Interno: visible solo para Infrastructure y las pruebas (InternalsVisibleTo).</summary>
    internal static User Restore(
        Guid id,
        Email email,
        PasswordHash passwordHash,
        UserRole role,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);
        if (!Enum.IsDefined(role))
        {
            throw new DomainException("El rol del usuario no es válido.");
        }

        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var updated = Guard.Utc(updatedAtUtc, nameof(updatedAtUtc));
        if (updated < created)
        {
            throw new DomainException("Las fechas del usuario no son coherentes.");
        }

        var name = displayName is null ? null : NormalizeDisplayName(displayName);
        return new User(Guard.NotEmpty(id, nameof(id)), email, passwordHash, role, created, updated, name);
    }

    /// <summary>Reemplaza el hash (rehash al autenticar cuando suben los parámetros de Argon2id).</summary>
    public void ChangePasswordHash(PasswordHash newHash, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(newHash);
        PasswordHash = newHash;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Cambia el nombre visible (pm-038). Se recorta y debe tener de 1 a 80 caracteres sin caracteres de control.</summary>
    public void ChangeDisplayName(string displayName, DateTime nowUtc)
    {
        DisplayName = NormalizeDisplayName(displayName);
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Indica si el valor sería un nombre visible aceptable, sin lanzar excepción (para validadores).</summary>
    public static bool IsValidDisplayName(string? value)
    {
        var trimmed = value?.Trim();
        return !string.IsNullOrEmpty(trimmed)
            && trimmed.Length <= MaxDisplayNameLength
            && !trimmed.Any(char.IsControl);
    }

    private static string NormalizeDisplayName(string value)
    {
        if (!IsValidDisplayName(value))
        {
            throw new DomainException("El nombre no es válido.");
        }

        return value.Trim();
    }
}
