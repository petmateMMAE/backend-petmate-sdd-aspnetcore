using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Carnet;

namespace PetMate.Application.Carnet;

// Casos de uso del carnet de salud (pm-097 a pm-103). Todos reciben el dueño tomado del token como primer miembro y
// comprueban la propiedad de la mascota antes de leer o escribir: una mascota o un registro ajenos responden NotFound.

// ---------- Vacunas (pm-097, pm-098) ----------

public sealed record VaccineResult(Guid Id, Guid PetId, DateOnly AppliedOn, string Name, string? Batch, string? AppliedBy, DateOnly? NextDoseOn, RecordOriginResult Origin, DateTime CreatedAtUtc)
{
    public static VaccineResult From(Vaccine v)
    {
        ArgumentNullException.ThrowIfNull(v);
        return new(v.Id, v.PetId, v.AppliedOn, v.Name, v.Batch, v.AppliedBy, v.NextDoseOn, RecordOriginResult.From(v.Provenance), v.CreatedAtUtc);
    }
}

public sealed record CreateVaccineCommand(Guid OwnerId, Guid PetId, VaccineDetails Details) : ICommand<CarnetResult<VaccineResult>>;

public sealed record ListVaccinesQuery(Guid OwnerId, Guid PetId, int? Page, int? PageSize) : IQuery<CarnetResult<PagedResult<VaccineResult>>>;

public sealed record DeleteVaccineCommand(Guid OwnerId, Guid VaccineId) : ICommand<CarnetStatus>;

public sealed class CreateVaccineHandler(IPetRepository pets, IVaccineRepository vaccines, TimeProvider timeProvider)
    : IRequestHandler<CreateVaccineCommand, CarnetResult<VaccineResult>>
{
    public Task<CarnetResult<VaccineResult>> Handle(CreateVaccineCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.Create(
            pets,
            vaccines,
            request.OwnerId,
            request.PetId,
            () => Vaccine.Create(request.PetId, request.OwnerId, RecordProvenance.ByOwner, request.Details, CarnetFlow.Today(timeProvider), timeProvider.GetUtcNow().UtcDateTime),
            VaccineResult.From,
            cancellationToken);
    }
}

public sealed class ListVaccinesHandler(IPetRepository pets, IVaccineRepository vaccines)
    : IRequestHandler<ListVaccinesQuery, CarnetResult<PagedResult<VaccineResult>>>
{
    public Task<CarnetResult<PagedResult<VaccineResult>>> Handle(ListVaccinesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.List(pets, vaccines, request.OwnerId, request.PetId, request.Page, request.PageSize, VaccineResult.From, cancellationToken);
    }
}

public sealed class DeleteVaccineHandler(IVaccineRepository vaccines) : IRequestHandler<DeleteVaccineCommand, CarnetStatus>
{
    public Task<CarnetStatus> Handle(DeleteVaccineCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.DeleteOwn(vaccines, request.OwnerId, request.VaccineId, cancellationToken);
    }
}

public sealed class CreateVaccineCommandValidator(TimeProvider timeProvider) : IRequestValidator<CreateVaccineCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateVaccineCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetFlow.ToFailures(request.Details.InvalidFields(CarnetFlow.Today(timeProvider)), request.PetId));
    }
}

public sealed class ListVaccinesQueryValidator : IRequestValidator<ListVaccinesQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListVaccinesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetValidation.ListFailures(request.PetId, request.Page, request.PageSize));
    }
}

// ---------- Desparasitaciones (pm-099, pm-100) ----------

public sealed record DewormingResult(Guid Id, Guid PetId, DateOnly AppliedOn, string Product, DewormingType? Type, string? AppliedBy, DateOnly? NextOn, RecordOriginResult Origin, DateTime CreatedAtUtc)
{
    public static DewormingResult From(Deworming d)
    {
        ArgumentNullException.ThrowIfNull(d);
        return new(d.Id, d.PetId, d.AppliedOn, d.Product, d.Type, d.AppliedBy, d.NextOn, RecordOriginResult.From(d.Provenance), d.CreatedAtUtc);
    }
}

public sealed record CreateDewormingCommand(Guid OwnerId, Guid PetId, DewormingDetails Details) : ICommand<CarnetResult<DewormingResult>>;

public sealed record ListDewormingsQuery(Guid OwnerId, Guid PetId, int? Page, int? PageSize) : IQuery<CarnetResult<PagedResult<DewormingResult>>>;

public sealed record DeleteDewormingCommand(Guid OwnerId, Guid DewormingId) : ICommand<CarnetStatus>;

public sealed class CreateDewormingHandler(IPetRepository pets, IDewormingRepository dewormings, TimeProvider timeProvider)
    : IRequestHandler<CreateDewormingCommand, CarnetResult<DewormingResult>>
{
    public Task<CarnetResult<DewormingResult>> Handle(CreateDewormingCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.Create(
            pets,
            dewormings,
            request.OwnerId,
            request.PetId,
            () => Deworming.Create(request.PetId, request.OwnerId, RecordProvenance.ByOwner, request.Details, CarnetFlow.Today(timeProvider), timeProvider.GetUtcNow().UtcDateTime),
            DewormingResult.From,
            cancellationToken);
    }
}

public sealed class ListDewormingsHandler(IPetRepository pets, IDewormingRepository dewormings)
    : IRequestHandler<ListDewormingsQuery, CarnetResult<PagedResult<DewormingResult>>>
{
    public Task<CarnetResult<PagedResult<DewormingResult>>> Handle(ListDewormingsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.List(pets, dewormings, request.OwnerId, request.PetId, request.Page, request.PageSize, DewormingResult.From, cancellationToken);
    }
}

public sealed class DeleteDewormingHandler(IDewormingRepository dewormings) : IRequestHandler<DeleteDewormingCommand, CarnetStatus>
{
    public Task<CarnetStatus> Handle(DeleteDewormingCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.DeleteOwn(dewormings, request.OwnerId, request.DewormingId, cancellationToken);
    }
}

public sealed class CreateDewormingCommandValidator(TimeProvider timeProvider) : IRequestValidator<CreateDewormingCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateDewormingCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetFlow.ToFailures(request.Details.InvalidFields(CarnetFlow.Today(timeProvider)), request.PetId));
    }
}

public sealed class ListDewormingsQueryValidator : IRequestValidator<ListDewormingsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListDewormingsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetValidation.ListFailures(request.PetId, request.Page, request.PageSize));
    }
}

// ---------- Pesos (pm-101) ----------

public sealed record WeightResult(Guid Id, Guid PetId, DateOnly MeasuredOn, decimal Kilograms, RecordOriginResult Origin, DateTime CreatedAtUtc)
{
    public static WeightResult From(WeightEntry w)
    {
        ArgumentNullException.ThrowIfNull(w);
        return new(w.Id, w.PetId, w.MeasuredOn, w.Kilograms, RecordOriginResult.From(w.Provenance), w.CreatedAtUtc);
    }
}

public sealed record CreateWeightCommand(Guid OwnerId, Guid PetId, WeightDetails Details) : ICommand<CarnetResult<WeightResult>>;

public sealed record ListWeightsQuery(Guid OwnerId, Guid PetId, int? Page, int? PageSize) : IQuery<CarnetResult<PagedResult<WeightResult>>>;

public sealed class CreateWeightHandler(IPetRepository pets, IWeightRepository weights, TimeProvider timeProvider)
    : IRequestHandler<CreateWeightCommand, CarnetResult<WeightResult>>
{
    public Task<CarnetResult<WeightResult>> Handle(CreateWeightCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.Create(
            pets,
            weights,
            request.OwnerId,
            request.PetId,
            () => WeightEntry.Create(request.PetId, request.OwnerId, RecordProvenance.ByOwner, request.Details, CarnetFlow.Today(timeProvider), timeProvider.GetUtcNow().UtcDateTime),
            WeightResult.From,
            cancellationToken);
    }
}

public sealed class ListWeightsHandler(IPetRepository pets, IWeightRepository weights)
    : IRequestHandler<ListWeightsQuery, CarnetResult<PagedResult<WeightResult>>>
{
    public Task<CarnetResult<PagedResult<WeightResult>>> Handle(ListWeightsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.List(pets, weights, request.OwnerId, request.PetId, request.Page, request.PageSize, WeightResult.From, cancellationToken);
    }
}

public sealed class CreateWeightCommandValidator(TimeProvider timeProvider) : IRequestValidator<CreateWeightCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateWeightCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetFlow.ToFailures(request.Details.InvalidFields(CarnetFlow.Today(timeProvider)), request.PetId));
    }
}

public sealed class ListWeightsQueryValidator : IRequestValidator<ListWeightsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListWeightsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetValidation.ListFailures(request.PetId, request.Page, request.PageSize));
    }
}

// ---------- Alergias y condiciones (pm-102, pm-103) ----------

public sealed record AllergyResult(Guid Id, Guid PetId, string Name, AllergyKind Kind, string? Notes, AllergyStatus Status, RecordOriginResult Origin, DateTime CreatedAtUtc, DateTime UpdatedAtUtc)
{
    public static AllergyResult From(Allergy a)
    {
        ArgumentNullException.ThrowIfNull(a);
        return new(a.Id, a.PetId, a.Name, a.Kind, a.Notes, a.Status, RecordOriginResult.From(a.Provenance), a.CreatedAtUtc, a.UpdatedAtUtc);
    }
}

public sealed record CreateAllergyCommand(Guid OwnerId, Guid PetId, AllergyDetails Details) : ICommand<CarnetResult<AllergyResult>>;

public sealed record ListAllergiesQuery(Guid OwnerId, Guid PetId, int? Page, int? PageSize) : IQuery<CarnetResult<PagedResult<AllergyResult>>>;

public sealed record UpdateAllergyCommand(Guid OwnerId, Guid AllergyId, AllergyDetails Details) : ICommand<CarnetResult<AllergyResult>>;

public sealed class CreateAllergyHandler(IPetRepository pets, IAllergyRepository allergies, TimeProvider timeProvider)
    : IRequestHandler<CreateAllergyCommand, CarnetResult<AllergyResult>>
{
    public Task<CarnetResult<AllergyResult>> Handle(CreateAllergyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.Create(
            pets,
            allergies,
            request.OwnerId,
            request.PetId,
            () => Allergy.Create(request.PetId, request.OwnerId, RecordProvenance.ByOwner, request.Details, timeProvider.GetUtcNow().UtcDateTime),
            AllergyResult.From,
            cancellationToken);
    }
}

public sealed class ListAllergiesHandler(IPetRepository pets, IAllergyRepository allergies)
    : IRequestHandler<ListAllergiesQuery, CarnetResult<PagedResult<AllergyResult>>>
{
    public Task<CarnetResult<PagedResult<AllergyResult>>> Handle(ListAllergiesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CarnetFlow.List(pets, allergies, request.OwnerId, request.PetId, request.Page, request.PageSize, AllergyResult.From, cancellationToken);
    }
}

public sealed class UpdateAllergyHandler(IAllergyRepository allergies, TimeProvider timeProvider)
    : IRequestHandler<UpdateAllergyCommand, CarnetResult<AllergyResult>>
{
    public async Task<CarnetResult<AllergyResult>> Handle(UpdateAllergyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var allergy = await allergies.FindOwned(request.AllergyId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (allergy is null)
        {
            return CarnetResult.Failed<AllergyResult>(CarnetStatus.NotFound);
        }

        if (!allergy.CanOwnerModify)
        {
            return CarnetResult.Failed<AllergyResult>(CarnetStatus.BusinessRecord);
        }

        allergy.Update(request.Details, timeProvider.GetUtcNow().UtcDateTime);
        return await allergies.UpdateOwned(allergy, cancellationToken).ConfigureAwait(false)
            ? CarnetResult.Ok(AllergyResult.From(allergy))
            : CarnetResult.Failed<AllergyResult>(CarnetStatus.NotFound);
    }
}

public sealed class CreateAllergyCommandValidator : IRequestValidator<CreateAllergyCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreateAllergyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetFlow.ToFailures(request.Details.InvalidFields(), request.PetId));
    }
}

/// <summary>La actualización exige el estado explícito: activa o superada (pm-103#CA-02).</summary>
public sealed class UpdateAllergyCommandValidator : IRequestValidator<UpdateAllergyCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateAllergyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fields = request.Details.InvalidFields().ToList();
        if (request.Details.Status is null)
        {
            fields.Add("status");
        }

        return Task.FromResult(CarnetFlow.ToFailures(fields, Guid.NewGuid(), includePet: false));
    }
}

public sealed class ListAllergiesQueryValidator : IRequestValidator<ListAllergiesQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListAllergiesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(CarnetValidation.ListFailures(request.PetId, request.Page, request.PageSize));
    }
}

/// <summary>Validación común de los listados del carnet: <c>petId</c> obligatorio más la convención de paginación (pm-047).</summary>
internal static class CarnetValidation
{
    public static IReadOnlyList<ValidationFailure> ListFailures(Guid petId, int? page, int? pageSize)
    {
        var failures = new List<ValidationFailure>();
        if (petId == Guid.Empty)
        {
            failures.Add(new ValidationFailure("petId", "La mascota es obligatoria."));
        }

        failures.AddRange(PaginationConvention.Check(page, pageSize));
        return failures;
    }
}
