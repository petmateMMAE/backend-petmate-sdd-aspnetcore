using PetMate.Application.Businesses;
using PetMate.Application.Carnet;
using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Businesses;
using PetMate.Domain.Carnet;
using PetMate.Domain.Pets;

namespace PetMate.Application.Access;

/// <summary>Consultas del carnet (pm-131 a pm-134). Origen siempre de negocio: de solo lectura para el dueño.</summary>
public interface IConsultationRepository : IHealthRecordRepository<Consultation>
{
    /// <summary>Consulta que registró ese negocio (la veterinaria autora); ajena o inexistente → <c>null</c>.</summary>
    Task<Consultation?> FindByIdForBusiness(Guid consultationId, Guid businessId, CancellationToken cancellationToken);

    /// <summary>Guarda los medicamentos de la receta; solo la veterinaria autora.</summary>
    Task<bool> ReplaceForBusiness(Consultation consultation, Guid businessId, CancellationToken cancellationToken);
}

/// <summary>Acceso de una veterinaria a una mascota: negocio, mascota y dueño, tras comprobar la autorización vigente.</summary>
public sealed record VeterinaryAccess(Business Business, Pet Pet, Guid OwnerId);

/// <summary>Comprobación común de las operaciones de una veterinaria sobre una mascota ajena (pm-131#CA-01).</summary>
internal static class AccessGate
{
    public static async Task<(AccessStatus Status, VeterinaryAccess? Access)> RequireVeterinary(
        IBusinessRepository businesses,
        IAccessGrantRepository grants,
        IPetRepository pets,
        Guid accountId,
        Guid petId,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var business = await businesses.FindByOwner(accountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return (AccessStatus.NoBusiness, null);
        }

        if (business.Type != BusinessType.Veterinary)
        {
            return (AccessStatus.NotVeterinary, null);
        }

        var active = await grants.FindActive(petId, business.Id, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
        if (active.Count == 0)
        {
            return (AccessStatus.Forbidden, null);
        }

        var ownerId = active[0].OwnerId;
        var pet = await pets.FindOwned(petId, ownerId, cancellationToken).ConfigureAwait(false);
        return pet is null ? (AccessStatus.NotFound, null) : (AccessStatus.Ok, new VeterinaryAccess(business, pet, ownerId));
    }
}

public sealed record MedicationResult(string Name, string Dose, string Frequency, string Duration, string? Instructions);

public sealed record ConsultationResult(Guid Id, Guid PetId, DateOnly ConsultedOn, RecordOriginResult Origin, string Reason, string Diagnosis, decimal? WeightKg, IReadOnlyList<MedicationResult> Medications, DateTime CreatedAtUtc)
{
    public static ConsultationResult From(Consultation c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return new(c.Id, c.PetId, c.ConsultedOn, RecordOriginResult.From(c.Provenance), c.Reason, c.Diagnosis, c.WeightKg, c.Medications.Select(m => new MedicationResult(m.Name, m.Dose, m.Frequency, m.Duration, m.Instructions)).ToArray(), c.CreatedAtUtc);
    }
}

/// <summary>Elemento del listado de consultas del dueño (pm-133#CA-02): fecha, veterinaria, motivo y diagnóstico.</summary>
public sealed record ConsultationSummary(Guid Id, DateOnly ConsultedOn, string? BusinessName, string Reason, string Diagnosis)
{
    public static ConsultationSummary From(Consultation c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return new(c.Id, c.ConsultedOn, c.Provenance.BusinessName, c.Reason, c.Diagnosis);
    }
}

// ---------- Consultas (pm-131, pm-132) ----------

public sealed record CreateConsultationCommand(Guid AccountId, Guid PetId, ConsultationDetails Details) : ICommand<AccessOutcome<ConsultationResult>>;

public sealed record AddMedicationCommand(Guid AccountId, Guid ConsultationId, MedicationDetails Medication) : ICommand<AccessOutcome<ConsultationResult>>;

public sealed class CreateConsultationHandler(
    IBusinessRepository businesses, IAccessGrantRepository grants, IPetRepository pets, IConsultationRepository consultations, TimeProvider timeProvider)
    : IRequestHandler<CreateConsultationCommand, AccessOutcome<ConsultationResult>>
{
    public async Task<AccessOutcome<ConsultationResult>> Handle(CreateConsultationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (status, access) = await AccessGate.RequireVeterinary(businesses, grants, pets, request.AccountId, request.PetId, timeProvider, cancellationToken).ConfigureAwait(false);
        if (access is null)
        {
            return AccessOutcome.Failed<ConsultationResult>(status);
        }

        if (await consultations.CountByPet(request.PetId, access.OwnerId, cancellationToken).ConfigureAwait(false) >= CarnetLimits.MaxRecordsPerPetAndType)
        {
            return AccessOutcome.Failed<ConsultationResult>(AccessStatus.Conflict);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var consultation = Consultation.Create(request.PetId, access.OwnerId, RecordProvenance.ByBusiness(access.Business.Id, access.Business.Name), request.Details, DateOnly.FromDateTime(now), now);
        await consultations.Add(consultation, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(ConsultationResult.From(consultation));
    }
}

/// <summary>Agrega un medicamento a la receta. Solo la veterinaria autora; el dueño nunca modifica la receta (pm-132#CA-03).</summary>
public sealed class AddMedicationHandler(IBusinessRepository businesses, IConsultationRepository consultations)
    : IRequestHandler<AddMedicationCommand, AccessOutcome<ConsultationResult>>
{
    public async Task<AccessOutcome<ConsultationResult>> Handle(AddMedicationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<ConsultationResult>(AccessStatus.NoBusiness);
        }

        var consultation = await consultations.FindByIdForBusiness(request.ConsultationId, business.Id, cancellationToken).ConfigureAwait(false);
        if (consultation is null)
        {
            return AccessOutcome.Failed<ConsultationResult>(AccessStatus.NotFound);
        }

        if (consultation.Medications.Count >= Consultation.MaxMedications)
        {
            return AccessOutcome.Failed<ConsultationResult>(AccessStatus.Conflict);
        }

        consultation.AddMedication(request.Medication);
        return await consultations.ReplaceForBusiness(consultation, business.Id, cancellationToken).ConfigureAwait(false)
            ? AccessOutcome.Ok(ConsultationResult.From(consultation))
            : AccessOutcome.Failed<ConsultationResult>(AccessStatus.NotFound);
    }
}

public sealed class CreateConsultationCommandValidator : IRequestValidator<CreateConsultationCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateConsultationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = request.Details.InvalidFields().Select(f => new ValidationFailure(f, BusinessCarnetMessages.For(f))).ToList();
        if (request.PetId == Guid.Empty)
        {
            failures.Insert(0, new ValidationFailure("petId", "La mascota es obligatoria."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

public sealed class AddMedicationCommandValidator : IRequestValidator<AddMedicationCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(AddMedicationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(request.Medication.InvalidFields().Select(f => new ValidationFailure(f, BusinessCarnetMessages.For(f))).ToArray());
    }
}

internal static class BusinessCarnetMessages
{
    public static string For(string field) => field switch
    {
        "reason" => $"El motivo es obligatorio y debe tener de 1 a {Consultation.MaxTextLength} caracteres.",
        "diagnosis" => $"El diagnóstico es obligatorio y debe tener de 1 a {Consultation.MaxTextLength} caracteres.",
        "weightKg" => $"El peso debe ser mayor que 0 y hasta {WeightEntry.MaxKilograms:0} kg, con un decimal.",
        "medications" => $"La receta admite hasta {Consultation.MaxMedications} medicamentos válidos (nombre, dosis, frecuencia y duración obligatorios).",
        "name" => "El nombre del medicamento es obligatorio.",
        "dose" => "La dosis es obligatoria.",
        "frequency" => "La frecuencia es obligatoria.",
        "duration" => "La duración es obligatoria.",
        "instructions" => $"Las indicaciones no deben exceder {Consultation.MaxInstructionsLength} caracteres.",
        _ => "El valor no es válido.",
    };
}

// ---------- Consultas del dueño (pm-133, pm-134) ----------

public sealed record ListConsultationsQuery(Guid OwnerId, Guid PetId, int? Page, int? PageSize) : IQuery<CarnetResult<PagedResult<ConsultationSummary>>>;

public sealed record GetConsultationQuery(Guid OwnerId, Guid ConsultationId) : IQuery<CarnetResult<ConsultationResult>>;

public sealed class ListConsultationsHandler(IPetRepository pets, IConsultationRepository consultations)
    : IRequestHandler<ListConsultationsQuery, CarnetResult<PagedResult<ConsultationSummary>>>
{
    public Task<CarnetResult<PagedResult<ConsultationSummary>>> Handle(ListConsultationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.List(pets, consultations, request.OwnerId, request.PetId, request.Page, request.PageSize, ConsultationSummary.From, cancellationToken);
    }
}

public sealed class GetConsultationHandler(IConsultationRepository consultations) : IRequestHandler<GetConsultationQuery, CarnetResult<ConsultationResult>>
{
    public async Task<CarnetResult<ConsultationResult>> Handle(GetConsultationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var consultation = await consultations.FindOwned(request.ConsultationId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        return consultation is null ? CarnetResult.Failed<ConsultationResult>(CarnetStatus.NotFound) : CarnetResult.Ok(ConsultationResult.From(consultation));
    }
}

public sealed class ListConsultationsQueryValidator : IRequestValidator<ListConsultationsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListConsultationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetValidation.ListFailures(request.PetId, request.Page, request.PageSize));
    }
}

// ---------- Vacunas y desparasitaciones aplicadas por la veterinaria (pm-135, pm-136) ----------

public sealed record CreateVaccineByBusinessCommand(Guid AccountId, Guid PetId, VaccineDetails Details) : ICommand<AccessOutcome<VaccineResult>>;

public sealed record CreateDewormingByBusinessCommand(Guid AccountId, Guid PetId, DewormingDetails Details) : ICommand<AccessOutcome<DewormingResult>>;

public sealed class CreateVaccineByBusinessHandler(
    IBusinessRepository businesses, IAccessGrantRepository grants, IPetRepository pets, IVaccineRepository vaccines, TimeProvider timeProvider)
    : IRequestHandler<CreateVaccineByBusinessCommand, AccessOutcome<VaccineResult>>
{
    public async Task<AccessOutcome<VaccineResult>> Handle(CreateVaccineByBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (status, access) = await AccessGate.RequireVeterinary(businesses, grants, pets, request.AccountId, request.PetId, timeProvider, cancellationToken).ConfigureAwait(false);
        if (access is null)
        {
            return AccessOutcome.Failed<VaccineResult>(status);
        }

        if (await vaccines.CountByPet(request.PetId, access.OwnerId, cancellationToken).ConfigureAwait(false) >= CarnetLimits.MaxRecordsPerPetAndType)
        {
            return AccessOutcome.Failed<VaccineResult>(AccessStatus.Conflict);
        }

        // «Aplicada por» siempre es el nombre del negocio (pm-135#CA-02): el cliente no puede enviarlo.
        var details = request.Details with { AppliedBy = access.Business.Name };
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var vaccine = Vaccine.Create(request.PetId, access.OwnerId, RecordProvenance.ByBusiness(access.Business.Id, access.Business.Name), details, DateOnly.FromDateTime(now), now);
        await vaccines.Add(vaccine, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(VaccineResult.From(vaccine));
    }
}

public sealed class CreateDewormingByBusinessHandler(
    IBusinessRepository businesses, IAccessGrantRepository grants, IPetRepository pets, IDewormingRepository dewormings, TimeProvider timeProvider)
    : IRequestHandler<CreateDewormingByBusinessCommand, AccessOutcome<DewormingResult>>
{
    public async Task<AccessOutcome<DewormingResult>> Handle(CreateDewormingByBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (status, access) = await AccessGate.RequireVeterinary(businesses, grants, pets, request.AccountId, request.PetId, timeProvider, cancellationToken).ConfigureAwait(false);
        if (access is null)
        {
            return AccessOutcome.Failed<DewormingResult>(status);
        }

        if (await dewormings.CountByPet(request.PetId, access.OwnerId, cancellationToken).ConfigureAwait(false) >= CarnetLimits.MaxRecordsPerPetAndType)
        {
            return AccessOutcome.Failed<DewormingResult>(AccessStatus.Conflict);
        }

        var details = request.Details with { AppliedBy = access.Business.Name };
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var deworming = Deworming.Create(request.PetId, access.OwnerId, RecordProvenance.ByBusiness(access.Business.Id, access.Business.Name), details, DateOnly.FromDateTime(now), now);
        await dewormings.Add(deworming, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(DewormingResult.From(deworming));
    }
}

public sealed class CreateVaccineByBusinessCommandValidator(TimeProvider timeProvider) : IRequestValidator<CreateVaccineByBusinessCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateVaccineByBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetFlow.ToFailures(request.Details.InvalidFields(CarnetFlow.Today(timeProvider)), request.PetId));
    }
}

public sealed class CreateDewormingByBusinessCommandValidator(TimeProvider timeProvider) : IRequestValidator<CreateDewormingByBusinessCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateDewormingByBusinessCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetFlow.ToFailures(request.Details.InvalidFields(CarnetFlow.Today(timeProvider)), request.PetId));
    }
}

// ---------- Mascotas atendidas (pm-130) ----------

/// <summary>Elemento de «mascotas atendidas»: foto, nombre y fecha del escaneo. No incluye datos clínicos ni da acceso a la ficha.</summary>
public sealed record AttendedPet(string PetName, string? PhotoUrl, DateTime ScannedAtUtc);

public enum AttendedPeriod
{
    All = 0,
    Day = 1,
    Week = 2,
}

public sealed record GetAttendedPetsQuery(Guid AccountId, string? Period, int? Page, int? PageSize) : IQuery<AccessOutcome<PagedResult<AttendedPet>>>;

public sealed class GetAttendedPetsHandler(IBusinessRepository businesses, IScanLogRepository scans, TimeProvider timeProvider)
    : IRequestHandler<GetAttendedPetsQuery, AccessOutcome<PagedResult<AttendedPet>>>
{
    public static bool TryParsePeriod(string? text, out AttendedPeriod period)
    {
        period = AttendedPeriod.All;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        switch (text)
        {
            case "day":
                period = AttendedPeriod.Day;
                return true;
            case "week":
                period = AttendedPeriod.Week;
                return true;
            default:
                return false;
        }
    }

    public async Task<AccessOutcome<PagedResult<AttendedPet>>> Handle(GetAttendedPetsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.AccountId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return AccessOutcome.Failed<PagedResult<AttendedPet>>(AccessStatus.NoBusiness);
        }

        // El validador ya comprobó el periodo; un valor inválido aquí significa «sin filtro».
        var period = TryParsePeriod(request.Period, out var parsedPeriod) ? parsedPeriod : AttendedPeriod.All;
        // «Hoy» y «esta semana» se cuentan en America/Mexico_City: el día local, o los últimos 7 días incluido hoy.
        var zone = WeeklySchedule.MexicoCity;
        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), zone);
        DateTime? since = period switch
        {
            AttendedPeriod.Day => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localNow.Date, DateTimeKind.Unspecified), zone),
            AttendedPeriod.Week => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localNow.Date.AddDays(-6), DateTimeKind.Unspecified), zone),
            _ => null,
        };
        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var total = await scans.CountByBusiness(business.Id, since, cancellationToken).ConfigureAwait(false);
        var skip = ((long)page - 1) * pageSize;
        if (skip >= total || skip > int.MaxValue)
        {
            return AccessOutcome.Ok(new PagedResult<AttendedPet>([], page, pageSize, total));
        }

        var logs = await scans.ListByBusiness(business.Id, since, (int)skip, pageSize, cancellationToken).ConfigureAwait(false);
        return AccessOutcome.Ok(new PagedResult<AttendedPet>(logs.Select(l => new AttendedPet(l.PetName, l.PhotoUrl, l.AtUtc)).ToArray(), page, pageSize, total));
    }
}

public sealed class GetAttendedPetsQueryValidator : IRequestValidator<GetAttendedPetsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(GetAttendedPetsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (!GetAttendedPetsHandler.TryParsePeriod(request.Period, out _))
        {
            failures.Add(new ValidationFailure("period", "El periodo debe ser day o week."));
        }

        failures.AddRange(PaginationConvention.Check(request.Page, request.PageSize));
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

// ---------- Historial médico unificado (pm-137) ----------

public enum HistoryKind
{
    Consultation = 1,
    Vaccine = 2,
    Deworming = 3,
    Weight = 4,
}

/// <summary>Elemento del historial: tipo, fecha y un título corto (motivo, vacuna, producto o peso).</summary>
public sealed record HistoryItem(HistoryKind Kind, Guid Id, DateOnly Date, string Title, RecordOriginResult Origin);

public sealed record GetHistoryQuery(Guid OwnerId, Guid PetId, string? Types, int? Page, int? PageSize) : IQuery<CarnetResult<PagedResult<HistoryItem>>>;

public sealed class GetHistoryHandler(
    IPetRepository pets, IConsultationRepository consultations, IVaccineRepository vaccines, IDewormingRepository dewormings, IWeightRepository weights)
    : IRequestHandler<GetHistoryQuery, CarnetResult<PagedResult<HistoryItem>>>
{
    public static bool TryParseKinds(string? text, out IReadOnlyList<HistoryKind> kinds)
    {
        var list = new List<HistoryKind>();
        kinds = list;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        foreach (var part in text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!Enum.TryParse<HistoryKind>(part, ignoreCase: false, out var kind) || !Enum.IsDefined(kind) || Enum.GetName(kind) != part)
            {
                return false;
            }

            list.Add(kind);
        }

        return true;
    }

    public async Task<CarnetResult<PagedResult<HistoryItem>>> Handle(GetHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false) is null)
        {
            return CarnetResult.Failed<PagedResult<HistoryItem>>(CarnetStatus.NotFound);
        }

        var kinds = TryParseKinds(request.Types, out var parsedKinds) ? parsedKinds : [];
        bool Wants(HistoryKind k) => kinds.Count == 0 || kinds.Contains(k);
        var max = CarnetLimits.MaxRecordsPerPetAndType;
        var items = new List<HistoryItem>();
        if (Wants(HistoryKind.Consultation))
        {
            items.AddRange((await consultations.ListByPet(request.PetId, request.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(c => new HistoryItem(HistoryKind.Consultation, c.Id, c.ConsultedOn, c.Reason, RecordOriginResult.From(c.Provenance))));
        }

        if (Wants(HistoryKind.Vaccine))
        {
            items.AddRange((await vaccines.ListByPet(request.PetId, request.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(v => new HistoryItem(HistoryKind.Vaccine, v.Id, v.AppliedOn, v.Name, RecordOriginResult.From(v.Provenance))));
        }

        if (Wants(HistoryKind.Deworming))
        {
            items.AddRange((await dewormings.ListByPet(request.PetId, request.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(d => new HistoryItem(HistoryKind.Deworming, d.Id, d.AppliedOn, d.Product, RecordOriginResult.From(d.Provenance))));
        }

        if (Wants(HistoryKind.Weight))
        {
            items.AddRange((await weights.ListByPet(request.PetId, request.OwnerId, 0, max, cancellationToken).ConfigureAwait(false)).Select(w => new HistoryItem(HistoryKind.Weight, w.Id, w.MeasuredOn, FormattableString.Invariant($"{w.Kilograms:0.0} kg"), RecordOriginResult.From(w.Provenance))));
        }

        var ordered = items.OrderByDescending(i => i.Date).ThenByDescending(i => i.Id).ToArray();
        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var skip = ((long)page - 1) * pageSize;
        var slice = skip >= ordered.Length ? [] : ordered.Skip((int)skip).Take(pageSize).ToArray();
        return CarnetResult.Ok(new PagedResult<HistoryItem>(slice, page, pageSize, ordered.Length));
    }
}

public sealed class GetHistoryQueryValidator : IRequestValidator<GetHistoryQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(GetHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = CarnetValidation.ListFailures(request.PetId, request.Page, request.PageSize).ToList();
        if (!GetHistoryHandler.TryParseKinds(request.Types, out _))
        {
            failures.Add(new ValidationFailure("types", "Los tipos deben ser Consultation, Vaccine, Deworming o Weight separados por comas."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}
