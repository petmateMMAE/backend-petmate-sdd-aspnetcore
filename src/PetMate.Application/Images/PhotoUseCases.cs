using System.Globalization;
using PetMate.Application.Businesses;
using PetMate.Application.Mediator;
using PetMate.Application.Persistence;

namespace PetMate.Application.Images;

/// <summary>
/// Fotos de mascotas y de negocios (pm-080, pm-081, pm-111). La clave del objeto la arma siempre el servidor a
/// partir de identificadores, nunca del nombre que mandó el cliente, y es fija por mascota o negocio: subir otra
/// vez reemplaza la anterior y nunca quedan objetos huérfanos.
/// </summary>
public static class PhotoKeys
{
    /// <summary>Duración de la URL temporal de una foto privada (pm-081).</summary>
    public static readonly TimeSpan TemporaryUrlLifetime = TimeSpan.FromMinutes(15);

    public static string ForPet(Guid ownerId, Guid petId)
        => string.Create(CultureInfo.InvariantCulture, $"{ownerId:N}/{petId:N}");

    public static string ForBusiness(Guid businessId) => businessId.ToString("N", CultureInfo.InvariantCulture);
}

/// <summary>Foto privada de una mascota, lista para mostrarse hasta <see cref="ExpiresAtUtc"/>.</summary>
public sealed record PetPhotoResult(Guid PetId, Uri Url, DateTimeOffset ExpiresAtUtc);

/// <summary>pm-080: sube o reemplaza la foto de una mascota propia. El contenido ya viene validado.</summary>
public sealed record UploadPetPhotoCommand(Guid OwnerId, Guid PetId, string ContentType, Stream Content) : ICommand<PetPhotoResult?>;

/// <summary>pm-080: quita la foto de una mascota propia.</summary>
public sealed record DeletePetPhotoCommand(Guid OwnerId, Guid PetId) : ICommand<bool>;

/// <summary>pm-081: URL temporal de la foto de una mascota propia.</summary>
public sealed record GetPetPhotoUrlQuery(Guid OwnerId, Guid PetId) : IQuery<PetPhotoResult?>;

/// <summary>pm-111: sube o reemplaza la foto del negocio de la cuenta.</summary>
public sealed record UploadBusinessPhotoCommand(Guid OwnerId, string ContentType, Stream Content) : ICommand<BusinessOutcome<Uri>>;

/// <summary>pm-111: quita la foto del negocio de la cuenta.</summary>
public sealed record DeleteBusinessPhotoCommand(Guid OwnerId) : ICommand<BusinessStatus>;

/// <summary>
/// Guarda la foto en el contenedor privado y la asocia a la mascota. Devuelve <c>null</c> si la mascota no existe
/// o es de otro propietario: el mismo resultado para los dos casos, para no revelar cuál (pm-080#CA-03).
/// </summary>
public sealed class UploadPetPhotoHandler(IPetRepository pets, IImageStorage storage, TimeProvider timeProvider)
    : IRequestHandler<UploadPetPhotoCommand, PetPhotoResult?>
{
    public async Task<PetPhotoResult?> Handle(UploadPetPhotoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (pet is null)
        {
            return null;
        }

        var key = PhotoKeys.ForPet(request.OwnerId, request.PetId);
        await storage.Save(ImageContainer.PetPhotos, key, request.ContentType, request.Content, cancellationToken).ConfigureAwait(false);

        pet.AttachPhoto(key, timeProvider.GetUtcNow().UtcDateTime);
        if (!await pets.UpdateOwned(pet, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var url = await storage
            .CreateTemporaryUrl(ImageContainer.PetPhotos, key, PhotoKeys.TemporaryUrlLifetime, cancellationToken)
            .ConfigureAwait(false);

        return url is null ? null : new PetPhotoResult(pet.Id, url.Url, url.ExpiresAtUtc);
    }
}

/// <summary>Quita la foto y borra el objeto. Devuelve <c>false</c> si la mascota no existe o es de otro propietario.</summary>
public sealed class DeletePetPhotoHandler(IPetRepository pets, IImageStorage storage, TimeProvider timeProvider)
    : IRequestHandler<DeletePetPhotoCommand, bool>
{
    public async Task<bool> Handle(DeletePetPhotoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (pet is null)
        {
            return false;
        }

        if (pet.PhotoObjectKey is { } key)
        {
            await storage.Delete(ImageContainer.PetPhotos, key, cancellationToken).ConfigureAwait(false);
        }

        pet.RemovePhoto(timeProvider.GetUtcNow().UtcDateTime);
        return await pets.UpdateOwned(pet, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Emite la URL temporal (pm-081). La propiedad se comprueba antes de firmar: una mascota ajena o inexistente
/// devuelve <c>null</c> y no se firma nada (pm-081#CA-02).
/// </summary>
public sealed class GetPetPhotoUrlHandler(IPetRepository pets, IImageStorage storage)
    : IRequestHandler<GetPetPhotoUrlQuery, PetPhotoResult?>
{
    public async Task<PetPhotoResult?> Handle(GetPetPhotoUrlQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var pet = await pets.FindOwned(request.PetId, request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (pet?.PhotoObjectKey is not { } key)
        {
            return null;
        }

        var url = await storage
            .CreateTemporaryUrl(ImageContainer.PetPhotos, key, PhotoKeys.TemporaryUrlLifetime, cancellationToken)
            .ConfigureAwait(false);

        return url is null ? null : new PetPhotoResult(pet.Id, url.Url, url.ExpiresAtUtc);
    }
}

/// <summary>
/// Guarda la foto del negocio de la cuenta en el contenedor público (pm-111). La URL lleva una marca de tiempo
/// para que al reemplazarla nadie siga viendo la anterior desde su caché.
/// </summary>
public sealed class UploadBusinessPhotoHandler(
    IBusinessRepository businesses,
    IImageStorage storage,
    TimeProvider timeProvider,
    PublicBusinessCache? publicCache = null) : IRequestHandler<UploadBusinessPhotoCommand, BusinessOutcome<Uri>>
{
    public async Task<BusinessOutcome<Uri>> Handle(UploadBusinessPhotoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessOutcome.Failed<Uri>(BusinessStatus.NoBusiness);
        }

        var now = timeProvider.GetUtcNow();
        var key = PhotoKeys.ForBusiness(business.Id);
        var stored = await storage
            .Save(ImageContainer.BusinessPhotos, key, request.ContentType, request.Content, cancellationToken)
            .ConfigureAwait(false);

        if (stored.PublicUrl is not { } blobUrl)
        {
            return BusinessOutcome.Failed<Uri>(BusinessStatus.NoBusiness);
        }

        var version = now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var url = new UriBuilder(blobUrl) { Query = "v=" + version }.Uri;

        business.SetPhoto(url.AbsoluteUri, now.UtcDateTime);
        if (!await businesses.UpdateOwned(business, cancellationToken).ConfigureAwait(false))
        {
            return BusinessOutcome.Failed<Uri>(BusinessStatus.NoBusiness);
        }

        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessOutcome.Ok(url);
    }
}

/// <summary>Quita la foto del negocio y borra el objeto público.</summary>
public sealed class DeleteBusinessPhotoHandler(
    IBusinessRepository businesses,
    IImageStorage storage,
    TimeProvider timeProvider,
    PublicBusinessCache? publicCache = null) : IRequestHandler<DeleteBusinessPhotoCommand, BusinessStatus>
{
    public async Task<BusinessStatus> Handle(DeleteBusinessPhotoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var business = await businesses.FindByOwner(request.OwnerId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return BusinessStatus.NoBusiness;
        }

        await storage
            .Delete(ImageContainer.BusinessPhotos, PhotoKeys.ForBusiness(business.Id), cancellationToken)
            .ConfigureAwait(false);

        business.SetPhoto(null, timeProvider.GetUtcNow().UtcDateTime);
        if (!await businesses.UpdateOwned(business, cancellationToken).ConfigureAwait(false))
        {
            return BusinessStatus.NoBusiness;
        }

        await BusinessCacheInvalidation.Invalidate(publicCache, cancellationToken).ConfigureAwait(false);
        return BusinessStatus.Ok;
    }
}
