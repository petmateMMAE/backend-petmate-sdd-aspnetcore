using PetMate.Application.Carnet;
using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Validation;
using PetMate.Domain.Pets;

namespace PetMate.Application.Pets;

/// <summary>Mascota tal como se devuelve al propietario. No incluye el identificador del propietario.</summary>
public sealed record PetResult(
    Guid Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    PetSex Sex,
    string? PhotoUrl,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    internal static PetResult From(Pet pet)
        => new(pet.Id, pet.Name, pet.Species, pet.Breed, pet.BirthDate, pet.Sex, pet.PhotoUrl, pet.CreatedAtUtc, pet.UpdatedAtUtc);
}

/// <summary>pm-040: crea una mascota. El propietario sale del Access Token validado, nunca del cuerpo.</summary>
public sealed record CreatePetCommand(Guid OwnerId, PetDetails Details) : ICommand<CreatePetOutcome>;

/// <summary>Límites de uso del módulo de mascotas (pm-045 CA-04).</summary>
public static class PetLimits
{
    /// <summary>Máximo de mascotas por propietario; protege la cuota de Atlas frente a creaciones en bucle.</summary>
    public const int MaxPetsPerOwner = 100;
}

/// <summary>Resultado de crear: la mascota, o <c>null</c> con <see cref="LimitReached"/> si el propietario ya tiene el máximo.</summary>
public sealed record CreatePetOutcome(PetResult? Pet, bool LimitReached);

/// <summary>pm-042: detalle de una mascota propia.</summary>
public sealed record GetMyPetQuery(Guid OwnerId, Guid PetId) : IQuery<PetResult?>;

/// <summary>pm-041: lista paginada de las mascotas del propietario (convención de pm-047; <c>null</c> = valor por defecto).</summary>
public sealed record ListMyPetsQuery(Guid OwnerId, int? Page, int? PageSize) : IQuery<PagedResult<PetResult>>;

/// <summary>pm-043: reemplaza los datos editables de una mascota propia.</summary>
public sealed record UpdateMyPetCommand(Guid OwnerId, Guid PetId, PetDetails Details) : ICommand<PetResult?>;

/// <summary>pm-044: elimina físicamente una mascota propia.</summary>
public sealed record DeleteMyPetCommand(Guid OwnerId, Guid PetId) : ICommand<bool>;

public sealed class CreatePetHandler(IPetRepository pets, TimeProvider timeProvider) : IRequestHandler<CreatePetCommand, CreatePetOutcome>
{
    public async Task<CreatePetOutcome> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        // La cuenta y el alta no son atómicas: creaciones simultáneas pueden pasar el tope por muy poco; el límite de
        // peticiones por usuario acota ese margen (el objetivo es frenar el abuso, no llevar un contador exacto).
        if (await pets.CountByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false) >= PetLimits.MaxPetsPerOwner)
        {
            return new CreatePetOutcome(null, LimitReached: true);
        }

        var pet = Pet.Create(request.OwnerId, request.Details, timeProvider.GetUtcNow().UtcDateTime);
        await pets.Add(pet, cancellationToken).ConfigureAwait(false);
        return new CreatePetOutcome(PetResult.From(pet), LimitReached: false);
    }
}

/// <summary>Devuelve <c>null</c> si la mascota no existe o es de otro propietario (mismo resultado: no se revela cuál).</summary>
public sealed class GetMyPetHandler(IPetRepository pets) : IRequestHandler<GetMyPetQuery, PetResult?>
{
    public async Task<PetResult?> Handle(GetMyPetQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        return pet is null ? null : PetResult.From(pet);
    }
}

/// <summary>Solo las mascotas del propietario del token, de la más reciente a la más antigua, con el total.</summary>
public sealed class ListMyPetsHandler(IPetRepository pets) : IRequestHandler<ListMyPetsQuery, PagedResult<PetResult>>
{
    public async Task<PagedResult<PetResult>> Handle(ListMyPetsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var total = await pets.CountByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);

        // Una página más allá de lo representable queda vacía sin consultar (evita el desbordamiento del desplazamiento).
        var skip = ((long)page - 1) * pageSize;
        if (skip >= total || skip > int.MaxValue)
        {
            return new PagedResult<PetResult>([], page, pageSize, total);
        }

        var items = await pets.ListByOwner(request.OwnerId, (int)skip, pageSize, cancellationToken).ConfigureAwait(false);
        return new PagedResult<PetResult>(items.Select(PetResult.From).ToArray(), page, pageSize, total);
    }
}

/// <summary>Devuelve <c>null</c> si la mascota no existe o es de otro propietario; si no, la mascota actualizada.</summary>
public sealed class UpdateMyPetHandler(IPetRepository pets, TimeProvider timeProvider) : IRequestHandler<UpdateMyPetCommand, PetResult?>
{
    public async Task<PetResult?> Handle(UpdateMyPetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (pet is null)
        {
            return null;
        }

        pet.Update(request.Details, timeProvider.GetUtcNow().UtcDateTime);
        return await pets.UpdateOwned(pet, cancellationToken).ConfigureAwait(false) ? PetResult.From(pet) : null;
    }
}

/// <summary>
/// Devuelve <c>false</c> si la mascota no existe o es de otro propietario; en ese caso no se borra nada. Si existe, borra en
/// cascada su carnet (pm-104): primero los registros hijos y al final la mascota, que lleva el QR. Cada paso es idempotente: si
/// uno falla, la mascota sigue existiendo y repetir la eliminación termina el trabajo, de modo que nunca quedan registros
/// huérfanos de una mascota que ya no existe (pm-104#CA-03).
/// </summary>
public sealed class DeleteMyPetHandler(
    IPetRepository pets,
    IVaccineRepository vaccines,
    IDewormingRepository dewormings,
    IWeightRepository weights,
    IAllergyRepository allergies) : IRequestHandler<DeleteMyPetCommand, bool>
{
    public async Task<bool> Handle(DeleteMyPetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false) is null)
        {
            return false;
        }

        await vaccines.DeleteByPet(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        await dewormings.DeleteByPet(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        await weights.DeleteByPet(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        await allergies.DeleteByPet(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        return await pets.DeleteOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Valida <c>page</c> y <c>pageSize</c> según la convención de pm-047.</summary>
public sealed class ListMyPetsQueryValidator : IRequestValidator<ListMyPetsQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ListMyPetsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PaginationConvention.Check(request.Page, request.PageSize));
    }
}

/// <summary>Valida los datos de la mascota antes de crearla (reglas de pm-039, fecha de hoy según el reloj del servidor).</summary>
public sealed class CreatePetCommandValidator(TimeProvider timeProvider) : IRequestValidator<CreatePetCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(CreatePetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PetValidation.Check(request.Details, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)));
    }
}

/// <summary>Valida los datos de la mascota antes de actualizarla (mismas reglas que al crear).</summary>
public sealed class UpdateMyPetCommandValidator(TimeProvider timeProvider) : IRequestValidator<UpdateMyPetCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateMyPetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PetValidation.Check(request.Details, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)));
    }
}
