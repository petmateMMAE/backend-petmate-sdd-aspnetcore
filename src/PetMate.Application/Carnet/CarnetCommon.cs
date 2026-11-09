using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Carnet;

namespace PetMate.Application.Carnet;

/// <summary>Límites del carnet (decisión del usuario, 2026-11-09).</summary>
public static class CarnetLimits
{
    /// <summary>Máximo de registros por mascota y tipo (vacunas, desparasitaciones, pesos, alergias).</summary>
    public const int MaxRecordsPerPetAndType = 200;

    /// <summary>Mensaje cuando el dueño intenta modificar o borrar un registro creado por un negocio.</summary>
    public const string BusinessRecordMessage = "No se puede modificar ni borrar: lo registró el negocio.";
}

/// <summary>Resultado de una operación del carnet. <c>NotFound</c> cubre mascota o registro ajenos e inexistentes (mismo resultado).</summary>
public enum CarnetStatus
{
    Ok = 0,
    NotFound = 1,
    LimitReached = 2,
    BusinessRecord = 3,
}

public sealed record CarnetResult<T>(CarnetStatus Status, T? Value = default);

/// <summary>Fábrica de <see cref="CarnetResult{T}"/>.</summary>
public static class CarnetResult
{
    public static CarnetResult<T> Ok<T>(T value) => new(CarnetStatus.Ok, value);

    public static CarnetResult<T> Failed<T>(CarnetStatus status) => new(status);
}

/// <summary>Origen del registro tal como se devuelve al dueño.</summary>
public sealed record RecordOriginResult(RecordOrigin Origin, Guid? BusinessId, string? BusinessName)
{
    public static RecordOriginResult From(RecordProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(provenance);
        return new RecordOriginResult(provenance.Origin, provenance.BusinessId, provenance.BusinessName);
    }
}

/// <summary>Repositorio común de los registros del carnet. Toda operación filtra por el dueño de la mascota.</summary>
public interface IHealthRecordRepository<TRecord>
    where TRecord : HealthRecord
{
    Task Add(TRecord record, CancellationToken cancellationToken);

    Task<TRecord?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TRecord>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken);

    Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken);

    Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Borra todos los registros de la mascota (cascada de pm-104). Devuelve cuántos borró.</summary>
    Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken);
}

public interface IVaccineRepository : IHealthRecordRepository<Vaccine>
{
}

public interface IDewormingRepository : IHealthRecordRepository<Deworming>
{
}

public interface IWeightRepository : IHealthRecordRepository<WeightEntry>
{
}

public interface IAllergyRepository : IHealthRecordRepository<Allergy>
{
    /// <summary>Guarda los cambios de una alergia del dueño; <c>false</c> si no existe, es ajena o la creó un negocio.</summary>
    Task<bool> UpdateOwned(Allergy allergy, CancellationToken cancellationToken);
}

/// <summary>Flujos comunes de los casos de uso del carnet: propiedad de la mascota, tope, paginación y borrado propio.</summary>
internal static class CarnetFlow
{
    public static DateOnly Today(TimeProvider timeProvider) => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    public static async Task<CarnetResult<TOut>> Create<TRecord, TOut>(
        IPetRepository pets,
        IHealthRecordRepository<TRecord> records,
        Guid ownerId,
        Guid petId,
        Func<TRecord> build,
        Func<TRecord, TOut> map,
        CancellationToken cancellationToken)
        where TRecord : HealthRecord
    {
        if (await pets.FindOwned(petId, ownerId, cancellationToken).ConfigureAwait(false) is null)
        {
            return CarnetResult.Failed<TOut>(CarnetStatus.NotFound);
        }

        // Como el tope de mascotas, la comprobación no es atómica; el límite por usuario acota el margen.
        if (await records.CountByPet(petId, ownerId, cancellationToken).ConfigureAwait(false) >= CarnetLimits.MaxRecordsPerPetAndType)
        {
            return CarnetResult.Failed<TOut>(CarnetStatus.LimitReached);
        }

        var record = build();
        await records.Add(record, cancellationToken).ConfigureAwait(false);
        return CarnetResult.Ok(map(record));
    }

    public static async Task<CarnetResult<PagedResult<TOut>>> List<TRecord, TOut>(
        IPetRepository pets,
        IHealthRecordRepository<TRecord> records,
        Guid ownerId,
        Guid petId,
        int? pageNumber,
        int? pageSizeNumber,
        Func<TRecord, TOut> map,
        CancellationToken cancellationToken)
        where TRecord : HealthRecord
    {
        if (await pets.FindOwned(petId, ownerId, cancellationToken).ConfigureAwait(false) is null)
        {
            return CarnetResult.Failed<PagedResult<TOut>>(CarnetStatus.NotFound);
        }

        var page = PaginationConvention.ResolvePage(pageNumber);
        var pageSize = PaginationConvention.ResolvePageSize(pageSizeNumber);
        var total = await records.CountByPet(petId, ownerId, cancellationToken).ConfigureAwait(false);
        var skip = ((long)page - 1) * pageSize;
        if (skip >= total || skip > int.MaxValue)
        {
            return CarnetResult.Ok(new PagedResult<TOut>([], page, pageSize, total));
        }

        var items = await records.ListByPet(petId, ownerId, (int)skip, pageSize, cancellationToken).ConfigureAwait(false);
        return CarnetResult.Ok(new PagedResult<TOut>(items.Select(map).ToArray(), page, pageSize, total));
    }

    public static async Task<CarnetStatus> DeleteOwn<TRecord>(
        IHealthRecordRepository<TRecord> records,
        Guid ownerId,
        Guid recordId,
        CancellationToken cancellationToken)
        where TRecord : HealthRecord
    {
        var record = await records.FindOwned(recordId, ownerId, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return CarnetStatus.NotFound;
        }

        if (!record.CanOwnerModify)
        {
            return CarnetStatus.BusinessRecord;
        }

        return await records.DeleteOwned(recordId, ownerId, cancellationToken).ConfigureAwait(false) ? CarnetStatus.Ok : CarnetStatus.NotFound;
    }

    public static IReadOnlyList<ValidationFailure> ToFailures(IReadOnlyList<string> invalidFields, Guid petId, bool includePet = true)
    {
        var failures = new List<ValidationFailure>();
        if (includePet && petId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("petId", "La mascota es obligatoria."));
        }

        foreach (var field in invalidFields)
        {
            failures.Add(new ValidationFailure(field, Messages.GetValueOrDefault(field, "El valor no es válido.")));
        }

        return failures;
    }

    // Mensajes en español, sin repetir el valor recibido.
    private static readonly Dictionary<string, string> Messages = new(StringComparer.Ordinal)
    {
        ["appliedOn"] = "La fecha de aplicación es obligatoria y no puede ser futura.",
        ["measuredOn"] = "La fecha de medición es obligatoria y no puede ser futura.",
        ["name"] = "El nombre es obligatorio y debe tener de 1 a 100 caracteres.",
        ["batch"] = $"El lote no debe exceder {Vaccine.MaxBatchLength} caracteres.",
        ["appliedBy"] = $"El campo «aplicada por» no debe exceder {Vaccine.MaxAppliedByLength} caracteres.",
        ["nextDoseOn"] = "La próxima dosis debe ser posterior a la fecha de aplicación.",
        ["product"] = $"El producto es obligatorio y debe tener de 1 a {Deworming.MaxProductLength} caracteres.",
        ["type"] = "El tipo debe ser Internal o External.",
        ["nextOn"] = "La próxima fecha debe ser futura.",
        ["kilograms"] = $"El peso debe ser mayor que 0 y hasta {WeightEntry.MaxKilograms:0} kg, con un decimal.",
        ["kind"] = "El tipo debe ser Allergy o Condition.",
        ["notes"] = $"Las notas no deben exceder {Allergy.MaxNotesLength} caracteres.",
        ["status"] = "El estado debe ser Active o Resolved.",
    };
}
