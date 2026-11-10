using PetMate.Domain.Common;

namespace PetMate.Domain.Users;

/// <summary>Cuenta de usuario. El rol no puede elegirse desde fuera: el alta pública crea siempre un User.</summary>
public sealed class User
{
    /// <summary>Longitud máxima del nombre visible (pm-038).</summary>
    public const int MaxDisplayNameLength = 80;

    /// <summary>Longitud del celular guardado: 10 dígitos (México), sin prefijo ni separadores (pm-091).</summary>
    public const int PhoneDigits = 10;

    private User(Guid id, Email email, PasswordHash passwordHash, UserRole role, DateTime createdAtUtc, DateTime updatedAtUtc, string? displayName, string? phone = null)
    {
        Phone = phone;
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

    /// <summary>Nombre con el que se muestra al usuario. Obligatorio en el alta (pm-085); puede ser nulo en cuentas anteriores.</summary>
    public string? DisplayName { get; private set; }

    /// <summary>Celular opcional (pm-091): se agrega después del registro, no se verifica por SMS. 10 dígitos o nulo.</summary>
    public string? Phone { get; private set; }

    /// <summary>Única fábrica de alta: toda cuenta nueva tiene el rol User; la solicitud no puede elegir ni elevar el rol.</summary>
    public static User Register(Email email, PasswordHash passwordHash, string displayName, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        return new User(Guid.CreateVersion7(), email, passwordHash, UserRole.User, now, now, NormalizeDisplayName(displayName));
    }

    /// <summary>Rehidrata un usuario ya persistido. Interno: visible solo para Infrastructure y las pruebas (InternalsVisibleTo).</summary>
    internal static User Restore(
        Guid id,
        Email email,
        PasswordHash passwordHash,
        UserRole role,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        string? displayName = null,
        string? phone = null)
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
        return new User(Guard.NotEmpty(id, nameof(id)), email, passwordHash, role, created, updated, name, phone is null ? null : NormalizePhoneOrThrow(phone));
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

    /// <summary>
    /// Cambia el celular (pm-091). Vacío o nulo lo quita. Acepta espacios, guiones, paréntesis y el prefijo +52 y lo guarda como
    /// 10 dígitos.
    /// </summary>
    public void ChangePhone(string? phone, DateTime nowUtc)
    {
        Phone = string.IsNullOrWhiteSpace(phone) ? null : NormalizePhoneOrThrow(phone);
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Indica si el celular es aceptable (vacío cuenta como «sin celular»); <paramref name="normalized"/> es nulo o 10 dígitos.</summary>
    public static bool TryNormalizePhone(string? value, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var digits = new System.Text.StringBuilder();
        var plus = false;
        foreach (var c in value.Trim())
        {
            if (c is >= '0' and <= '9')
            {
                digits.Append(c);
            }
            else if (c == '+' && digits.Length == 0 && !plus)
            {
                plus = true;
            }
            else if (c is not (' ' or '-' or '(' or ')' or '.'))
            {
                return false;
            }
        }

        var text = digits.ToString();
        if (text.Length == PhoneDigits + 2 && text.StartsWith("52", StringComparison.Ordinal))
        {
            text = text[2..];
        }
        else if (plus)
        {
            return false;
        }

        if (text.Length != PhoneDigits)
        {
            return false;
        }

        normalized = text;
        return true;
    }

    private static string NormalizePhoneOrThrow(string value)
        => TryNormalizePhone(value, out var normalized) && normalized is not null ? normalized : throw new DomainException("El celular no es válido.");

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
