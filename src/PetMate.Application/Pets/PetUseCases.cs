using PetMate.Application.Mediator;
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
public sealed record CreatePetCommand(Guid OwnerId, PetDetails Details) : ICommand<PetResult>;

/// <summary>pm-042: detalle de una mascota propia.</summary>
public sealed record GetMyPetQuery(Guid OwnerId, Guid PetId) : IQuery<PetResult?>;

/// <summary>pm-043: reemplaza los datos editables de una mascota propia.</summary>
public sealed record UpdateMyPetCommand(Guid OwnerId, Guid PetId, PetDetails Details) : ICommand<PetResult?>;

/// <summary>pm-044: elimina físicamente una mascota propia.</summary>
public sealed record DeleteMyPetCommand(Guid OwnerId, Guid PetId) : ICommand<bool>;

public sealed class CreatePetHandler(IPetRepository pets, TimeProvider timeProvider) : IRequestHandler<CreatePetCommand, PetResult>
{
    public async Task<PetResult> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = Pet.Create(request.OwnerId, request.Details, timeProvider.GetUtcNow().UtcDateTime);
        await pets.Add(pet, cancellationToken).ConfigureAwait(false);
        return PetResult.From(pet);
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

/// <summary>Devuelve <c>false</c> si la mascota no existe o es de otro propietario; en ese caso no se borra nada.</summary>
public sealed class DeleteMyPetHandler(IPetRepository pets) : IRequestHandler<DeleteMyPetCommand, bool>
{
    public Task<bool> Handle(DeleteMyPetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return pets.DeleteOwned(request.PetId, request.OwnerId, cancellationToken);
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
