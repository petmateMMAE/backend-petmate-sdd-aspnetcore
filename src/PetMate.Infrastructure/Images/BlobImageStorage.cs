using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using PetMate.Application.Images;

namespace PetMate.Infrastructure.Images;

/// <summary>
/// Adaptador de imágenes sobre Azure Blob Storage (pm-079). El contenido va directo de la petición al Blob: nada
/// toca el disco del contenedor de la API (pm-079#CA-02). El contenedor de mascotas es privado y solo se lee con
/// una SAS de corta duración (pm-081); el de negocios es público de solo lectura (pm-111).
/// </summary>
public sealed class BlobImageStorage(BlobServiceClient client, ImageStorageOptions options) : IImageStorage
{
    /// <summary>Margen hacia atrás al pedir la clave de delegación, por si los relojes no coinciden.</summary>
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    public bool IsEnabled => true;

    public async Task<StoredImage> Save(
        ImageContainer container,
        string objectKey,
        string contentType,
        Stream content,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        ArgumentNullException.ThrowIfNull(content);

        var blob = Blob(container, objectKey);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                // Una foto se reemplaza entera con otra clave, así que se puede cachear sin miedo.
                CacheControl = "public, max-age=86400",
            },
        };

        await blob.UploadAsync(content, uploadOptions, cancellationToken).ConfigureAwait(false);
        return new StoredImage(objectKey, container == ImageContainer.BusinessPhotos ? blob.Uri : null);
    }

    public async Task Delete(ImageContainer container, string objectKey, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        await Blob(container, objectKey)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TemporaryImageUrl?> CreateTemporaryUrl(
        ImageContainer container,
        string objectKey,
        TimeSpan lifetime,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);

        var blob = Blob(container, objectKey);
        if (!await blob.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        var builder = new BlobSasBuilder(BlobSasPermissions.Read, expiresAt)
        {
            BlobContainerName = Name(container),
            BlobName = objectKey,
            Resource = "b",
        };

        // Con cadena de conexión se firma con la llave de la cuenta; con identidad administrada no hay llave que
        // guardar, así que se pide una clave de delegación que caduca con la propia URL.
        if (blob.CanGenerateSasUri)
        {
            return new TemporaryImageUrl(blob.GenerateSasUri(builder), expiresAt);
        }

        var delegationKey = await client
            .GetUserDelegationKeyAsync(DateTimeOffset.UtcNow.Subtract(ClockSkew), expiresAt, cancellationToken)
            .ConfigureAwait(false);

        var query = builder.ToSasQueryParameters(delegationKey.Value, client.AccountName).ToString();
        return new TemporaryImageUrl(new UriBuilder(blob.Uri) { Query = query }.Uri, expiresAt);
    }

    private BlobClient Blob(ImageContainer container, string objectKey)
        => client.GetBlobContainerClient(Name(container)).GetBlobClient(objectKey);

    private string Name(ImageContainer container) => container switch
    {
        ImageContainer.BusinessPhotos => options.BusinessContainer,
        _ => options.PetContainer,
    };
}

/// <summary>
/// Adaptador que se registra cuando no hay almacenamiento configurado (desarrollo local y pruebas). Los endpoints
/// consultan <see cref="IsEnabled"/> y responden 503 antes de llamar a nada: estos métodos son solo una red de seguridad.
/// </summary>
public sealed class DisabledImageStorage : IImageStorage
{
    public bool IsEnabled => false;

    public Task<StoredImage> Save(ImageContainer container, string objectKey, string contentType, Stream content, CancellationToken cancellationToken)
        => throw new InvalidOperationException("El almacenamiento de imágenes no está configurado.");

    public Task Delete(ImageContainer container, string objectKey, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<TemporaryImageUrl?> CreateTemporaryUrl(ImageContainer container, string objectKey, TimeSpan lifetime, CancellationToken cancellationToken)
        => Task.FromResult<TemporaryImageUrl?>(null);
}
