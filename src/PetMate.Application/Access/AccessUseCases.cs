using PetMate.Application.Businesses;
using PetMate.Application.Carnet;
using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Application.Validation;
using PetMate.Domain.Access;
using PetMate.Domain.Businesses;
using PetMate.Domain.Carnet;
using PetMate.Domain.Pets;

namespace PetMate.Application.Access;

/// <summary>Autorizaciones de acceso (pm-122). No hay operación para listarlas ni revocarlas: el dueño solo las crea.</summary>
public interface IAccessGrantRepository
{
    Task Add(AccessGrant grant, CancellationToken cancellationToken);

    /// <summary>Autorizaciones vigentes de la mascota que valen para ese negocio (suyas o «cualquier negocio»).</summary>
    Task<IReadOnlyList<AccessGrant>> FindActive(Guid petId, Guid businessId, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Cancela todas las autorizaciones vigentes de la mascota del dueño (pm-124). Devuelve cuántas canceló.</summary>
    Task<long> CancelActiveForPet(Guid petId, Guid ownerId, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Borra todas las autorizaciones de la mascota (cascada, pm-104).</summary>
    Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken);
}

/// <summary>Registro interno de escaneos con acceso concedido (pm-129). Sin lectura para el dueño.</summary>
public interface IScanLogRepository
{
    Task Add(ScanLog log, CancellationToken cancellationToken);

    Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken);

    /// <summary>Escaneos del negocio desde la fecha dada (o todos), del más reciente al más antiguo (pm-130).</summary>
    Task<IReadOnlyList<ScanLog>> ListByBusiness(Guid businessId, DateTime? sinceUtc, int skip, int take, CancellationToken cancellationToken);

    Task<long> CountByBusiness(Guid businessId, DateTime? sinceUtc, CancellationToken cancellationToken);
}

public enum AccessStatus
{
    Ok = 0,

    /// <summary>Mascota ajena o inexistente, o código/token desconocido (mismo resultado).</summary>
    NotFound = 1,

    /// <summary>La cuenta no tiene un negocio asociado.</summary>
    NoBusiness = 2,

    /// <summary>No hay una autorización vigente que incluya al negocio: se rechaza sin datos de la mascota.</summary>
    Forbidden = 3,

    /// <summary>El negocio destinatario de la autorización no existe.</summary>
    BusinessNotFound = 4,

    TooManyRequests = 5,

    /// <summary>La cuenta tiene negocio pero no es una veterinaria (403).</summary>
    NotVeterinary = 6,

    /// <summary>Estado en conflicto: duplicado o tope alcanzado (409).</summary>
    Conflict = 7,

    /// <summary>La conversación está cerrada y no admite mensajes (409).</summary>
    Closed = 8,
}

public sealed record AccessOutcome<T>(AccessStatus Status, T? Value = default, TimeSpan? RetryAfter = null);

public static class AccessOutcome
{
    public static AccessOutcome<T> Ok<T>(T value) => new(AccessStatus.Ok, value);

    public static AccessOutcome<T> Failed<T>(AccessStatus status, TimeSpan? retryAfter = null) => new(status, default, retryAfter);
}

public sealed record AccessGrantResult(Guid Id, Guid PetId, Guid? BusinessId, IReadOnlyList<PetDataCategory> Data, DateTime CreatedAtUtc, DateTime ExpiresAtUtc)
{
    public static AccessGrantResult From(AccessGrant g)
    {
        ArgumentNullException.ThrowIfNull(g);
        return new(g.Id, g.PetId, g.BusinessId, g.Data, g.CreatedAtUtc, g.ExpiresAtUtc);
    }
}

public sealed record ScanAllergy(string Name, AllergyKind Kind, string? Notes);

/// <summary>
/// Ficha básica que recibe un negocio con autorización vigente (pm-128): foto, nombre, especie, raza, edad y las alergias activas.
/// El servidor no envía ningún otro campo. <c>PetId</c> solo se entrega a una veterinaria (lo necesita para registrar consultas).
/// </summary>
public sealed record ScanFicha(
    Guid? PetId,
    string? PhotoUrl,
    string Name,
    string Species,
    string? Breed,
    int? AgeYears,
    int? AgeMonths,
    IReadOnlyList<ScanAllergy> Allergies,
    IReadOnlyList<ScanVaccine>? Vaccines = null,
    IReadOnlyList<ScanDeworming>? Dewormings = null,
    IReadOnlyList<ScanWeight>? Weights = null,
    IReadOnlyList<ScanConsultation>? Consultations = null);

// Grupos de la ficha completa de una veterinaria (pm-127): solo se llenan los que el dueño autorizó; son de solo lectura.
public sealed record ScanVaccine(string Name, DateOnly AppliedOn, DateOnly? NextDoseOn, string? Batch, string? AppliedBy);

public sealed record ScanDeworming(string Product, DewormingType? Type, DateOnly AppliedOn, DateOnly? NextOn, string? AppliedBy);

public sealed record ScanWeight(DateOnly MeasuredOn, decimal Kilograms);

public sealed record ScanConsultation(Guid Id, DateOnly ConsultedOn, string? BusinessName, string Reason, string Diagnosis, IReadOnlyList<MedicationResult> Medications);

// ---------- Crear autorización (pm-123) ----------

public sealed record CreateAccessGrantCommand(Guid OwnerId, Guid PetId, Guid? BusinessId, IReadOnlyList<PetDataCategory> Data, int? DurationMinutes)
    : ICommand<AccessOutcome<AccessGrantResult>>;

public sealed class CreateAccessGrantHandler(IPetRepository pets, IBusinessRepository businesses, IAccessGrantRepository grants, TimeProvider timeProvider)
    : IRequestHandler<CreateAccessGrantCommand, AccessOutcome<AccessGrantResult>>
{
    public async Task<AccessOutcome<AccessGrantResult>> Handle(CreateAccessGrantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false) is null)
        {
            return AccessOutcome.Failed<AccessGrantResult>(AccessStatus.NotFound);
        }

        if (request.BusinessId is { } businessId && await businesses.FindById(businessId, cancellationToken).ConfigureAwait(false) is null)
        {
            return AccessOutcome.Failed<AccessGrantResult>(AccessStatus.BusinessNotFound);
        }

        var grant = AccessGrant.Create(request.PetId, request.OwnerId, request.BusinessId, request.Data, request.DurationMinutes!.Value, timeProvider.GetUtcNow().UtcDateTime);
        await grants.Add(grant, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(AccessGrantResult.From(grant));
    }
}

public sealed class CreateAccessGrantCommandValidator : IRequestValidator<CreateAccessGrantCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateAccessGrantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (request.PetId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("petId", "La mascota es obligatoria."));
        }

        if (request.BusinessId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("businessId", "El negocio no es válido."));
        }

        if (request.Data.Any(c => !Enum.IsDefined(c)))
        {
            failures.Add(new ValidationFailure("data", "Los datos permitidos deben ser Vaccines, Dewormings, Weights o Consultations."));
        }

        if (!AccessGrant.IsValidDuration(request.DurationMinutes))
        {
            failures.Add(new ValidationFailure("durationMinutes", "La duración debe ser 30 minutos, 1 día (1440) o 7 días (10080)."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

// ---------- Escanear (pm-125, pm-126, pm-128, pm-129) ----------

public sealed record ResolveScanCommand(Guid AccountId, string Code) : ICommand<AccessOutcome<ScanFicha>>;

/// <summary>
/// Un negocio escanea un QR (token) o escribe el código corto. Solo una cuenta con negocio puede hacerlo y los intentos se limitan
/// por negocio (impide enumerar códigos). Conocer el QR no basta: sin una autorización vigente que incluya al negocio se rechaza
/// con 403 y sin datos de la mascota. Cada acceso concedido deja un registro interno del escaneo.
/// </summary>
public sealed class ResolveScanHandler(
    IBusinessRepository businesses,
    IPetRepository pets,
    IAccessGrantRepository grants,
    IScanLogRepository scans,
    IAllergyRepository allergies,
    IAuthAttemptThrottle throttle,
    TimeProvider timeProvider,
    IVaccineRepository vaccines,
    IDewormingRepository dewormings,
    IWeightRepository weights,
    IConsultationRepository consultations,
    Images.IImageStorage images) : IRequestHandler<ResolveScanCommand, AccessOutcome<ScanFicha>>
{
    public async Task<AccessOutcome<ScanFicha>> Handle(ResolveScanCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<ScanFicha>(AccessStatus.NoBusiness);
        }

        var subject = business.Id.ToString("N");
        var wait = await throttle.GetRemainingLock(AuthThrottleRules.ScanScope, subject, cancellationToken).ConfigureAwait(false)
            ?? await throttle.RecordAttempt(AuthThrottleRules.ScanScope, subject, AuthThrottleRules.Scan, cancellationToken).ConfigureAwait(false);
        if (wait is { } remaining)
        {
            return AccessOutcome.Failed<ScanFicha>(AccessStatus.TooManyRequests, remaining);
        }

        var pet = await pets.FindByQr(request.Code.Trim(), cancellationToken).ConfigureAwait(false);
        if (pet is null)
        {
            return AccessOutcome.Failed<ScanFicha>(AccessStatus.NotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var active = await grants.FindActive(pet.Id, business.Id, now, cancellationToken).ConfigureAwait(false);
        if (active.Count == 0)
        {
            return AccessOutcome.Failed<ScanFicha>(AccessStatus.Forbidden);
        }

        // La foto subida es privada (pm-081): el negocio recibe una URL temporal, y solo porque hay autorización vigente.
        var photoUrl = pet.PhotoUrl;
        if (pet.PhotoObjectKey is { } photoKey)
        {
            var temporary = await images
                .CreateTemporaryUrl(Images.ImageContainer.PetPhotos, photoKey, Images.PhotoKeys.TemporaryUrlLifetime, cancellationToken)
                .ConfigureAwait(false);
            photoUrl = temporary?.Url.AbsoluteUri;
        }

        await scans.Add(ScanLog.Record(pet.Id, pet.Name, photoUrl, business.Id, now), cancellationToken).ConfigureAwait(false);

        var all = await allergies.ListByPet(pet.Id, pet.OwnerId, 0, CarnetLimits.MaxRecordsPerPetAndType, cancellationToken).ConfigureAwait(false);
        var activeAllergies = all.Where(a => a.Status == AllergyStatus.Active).Select(a => new ScanAllergy(a.Name, a.Kind, a.Notes)).ToArray();
        var (years, months) = Age(pet.BirthDate, DateOnly.FromDateTime(now));
        var ficha = new ScanFicha(business.Type == BusinessType.Veterinary ? pet.Id : null, photoUrl, pet.Name, pet.Species, pet.Breed, years, months, activeAllergies);
        if (business.Type != BusinessType.Veterinary)
        {
            // Tienda y estética reciben solo la ficha básica (pm-128).
            return AccessOutcome.Ok(ficha);
        }

        // Ficha completa de la veterinaria (pm-127): solo los grupos que alguna autorización vigente permite.
        var allowed = active.SelectMany(g => g.Data).ToHashSet();
        const int max = 50;
        return AccessOutcome.Ok(ficha with
        {
            Vaccines = allowed.Contains(PetDataCategory.Vaccines)
                ? (await vaccines.ListByPet(pet.Id, pet.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(v => new ScanVaccine(v.Name, v.AppliedOn, v.NextDoseOn, v.Batch, v.AppliedBy)).ToArray()
                : null,
            Dewormings = allowed.Contains(PetDataCategory.Dewormings)
                ? (await dewormings.ListByPet(pet.Id, pet.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(d => new ScanDeworming(d.Product, d.Type, d.AppliedOn, d.NextOn, d.AppliedBy)).ToArray()
                : null,
            Weights = allowed.Contains(PetDataCategory.Weights)
                ? (await weights.ListByPet(pet.Id, pet.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(w => new ScanWeight(w.MeasuredOn, w.Kilograms)).ToArray()
                : null,
            Consultations = allowed.Contains(PetDataCategory.Consultations)
                ? (await consultations.ListByPet(pet.Id, pet.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(c => new ScanConsultation(c.Id, c.ConsultedOn, c.Provenance.BusinessName, c.Reason, c.Diagnosis, c.Medications.Select(m => new MedicationResult(m.Name, m.Dose, m.Frequency, m.Duration, m.Instructions)).ToArray())).ToArray()
                : null,
        });
    }

    private static (int? Years, int? Months) Age(DateOnly? birth, DateOnly today)
    {
        if (birth is not { } b || b > today)
        {
            return (null, null);
        }

        var totalMonths = ((today.Year - b.Year) * 12) + today.Month - b.Month - (today.Day < b.Day ? 1 : 0);
        return (totalMonths / 12, totalMonths % 12);
    }
}

public sealed class ResolveScanCommandValidator : IRequestValidator<ResolveScanCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ResolveScanCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = request.Code.Trim();
        var valid = (code.Length == PetQr.CodeLength && code.ToUpperInvariant().All(c => PetQr.CodeAlphabet.Contains(c, StringComparison.Ordinal)))
            || (code.Length == PetQr.TokenLength && code.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'));
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(valid ? [] : [new ValidationFailure("code", "El código debe ser el token del QR o el código corto de 8 caracteres.")]);
    }
}
