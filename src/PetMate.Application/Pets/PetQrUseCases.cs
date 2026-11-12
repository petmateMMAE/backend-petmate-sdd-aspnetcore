using PetMate.Application.Mediator;
using PetMate.Application.Persistence;

namespace PetMate.Application.Pets;

/// <summary>QR de una mascota propia (pm-094, pm-095).</summary>
public sealed record PetQrResult(Guid PetId, string Token, string Code);

/// <summary>pm-094: lee el QR de una mascota propia.</summary>
public sealed record GetPetQrQuery(Guid OwnerId, Guid PetId) : IQuery<PetQrResult?>;

/// <summary>pm-095: genera un QR nuevo para una mascota propia; el anterior deja de ser válido.</summary>
public sealed record RegeneratePetQrCommand(Guid OwnerId, Guid PetId) : ICommand<PetQrResult?>;

/// <summary>Devuelve <c>null</c> si la mascota no existe o es de otro propietario (mismo resultado: no se revela cuál).</summary>
public sealed class GetPetQrHandler(IPetRepository pets) : IRequestHandler<GetPetQrQuery, PetQrResult?>
{
    public async Task<PetQrResult?> Handle(GetPetQrQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        return pet is null ? null : new PetQrResult(pet.Id, pet.Qr.Token, pet.Qr.Code);
    }
}

public sealed class RegeneratePetQrHandler(IPetRepository pets, Access.IAccessGrantRepository grants, TimeProvider timeProvider) : IRequestHandler<RegeneratePetQrCommand, PetQrResult?>
{
    public async Task<PetQrResult?> Handle(RegeneratePetQrCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (pet is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        // pm-124: primero se cancelan las autorizaciones y después se cambia el QR. Si algo falla entre ambos pasos, el QR sigue
        // siendo el anterior y no queda ninguna autorización viva con un QR nuevo.
        await grants.CancelActiveForPet(pet.Id, pet.OwnerId, now, cancellationToken).ConfigureAwait(false);
        pet.RegenerateQr(now);
        return await pets.ReplaceQrOwned(pet, cancellationToken).ConfigureAwait(false)
            ? new PetQrResult(pet.Id, pet.Qr.Token, pet.Qr.Code)
            : null;
    }
}
