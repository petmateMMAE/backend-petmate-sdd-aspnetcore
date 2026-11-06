using PetMate.Domain.Users;

namespace PetMate.Application.Persistence;

/// <summary>Persistencia de usuarios. Sin tipos del proveedor: la implementación vive en Infrastructure.</summary>
public interface IUserRepository
{
    /// <summary>Inserta el usuario. Devuelve <c>false</c> si el correo normalizado ya existe (sin lectura previa: sin condición de carrera).</summary>
    Task<bool> TryAdd(User user, CancellationToken cancellationToken);

    Task<User?> FindByEmail(Email email, CancellationToken cancellationToken);

    Task<User?> FindById(Guid id, CancellationToken cancellationToken);

    /// <summary>Persiste el hash y la fecha de actualización del usuario (rehash tras autenticar).</summary>
    Task UpdatePasswordHash(User user, CancellationToken cancellationToken);

    /// <summary>Persiste los datos de perfil editables (nombre visible) y la fecha de actualización (pm-038).</summary>
    Task UpdateProfile(User user, CancellationToken cancellationToken);
}
