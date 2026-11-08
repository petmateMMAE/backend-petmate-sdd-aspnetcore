using PetMate.Domain.Pets;

namespace PetMate.Application.Persistence;

/// <summary>
/// Persistencia de mascotas (pm-039). Todas las operaciones de lectura y escritura exigen el propietario: el filtro por
/// <c>OwnerId</c> vive en la propia consulta, de modo que un identificador ajeno se comporta igual que uno inexistente.
/// </summary>
public interface IPetRepository
{
    Task Add(Pet pet, CancellationToken cancellationToken);

    /// <summary>Devuelve la mascota solo si pertenece al propietario; si no existe o es ajena, <c>null</c>.</summary>
    Task<Pet?> FindOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Mascotas del propietario, de la más reciente a la más antigua, con desplazamiento y tamaño de página.</summary>
    Task<IReadOnlyList<Pet>> ListByOwner(Guid ownerId, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Total de mascotas del propietario (para la paginación, pm-047).</summary>
    Task<long> CountByOwner(Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Persiste los datos editables. Devuelve <c>false</c> si la mascota no existe o pertenece a otro propietario.</summary>
    Task<bool> UpdateOwned(Pet pet, CancellationToken cancellationToken);

    /// <summary>Elimina físicamente la mascota. Devuelve <c>false</c> si no existe o pertenece a otro propietario.</summary>
    Task<bool> DeleteOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken);
}
