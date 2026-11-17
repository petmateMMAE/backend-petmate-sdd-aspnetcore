using System.Collections.Concurrent;
using PetMate.Application.Images;

namespace PetMate.TestDoubles;

/// <summary>
/// Almacenamiento de imágenes en memoria (pm-079). Guarda los bytes en un diccionario y emite URLs falsas pero
/// con la forma correcta, para poder comprobar qué se guardó, qué se borró y cuándo caduca una URL.
/// </summary>
public sealed class InMemoryImageStorage : IImageStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _objects = new(StringComparer.Ordinal);

    public bool IsEnabled { get; set; } = true;

    /// <summary>Claves guardadas, con el prefijo del contenedor.</summary>
    public IReadOnlyCollection<string> Keys => [.. _objects.Keys];

    /// <summary>Claves que se pidió borrar, en orden.</summary>
    public List<string> Deleted { get; } = [];

    public byte[]? Content(ImageContainer container, string objectKey)
        => _objects.TryGetValue(Full(container, objectKey), out var bytes) ? bytes : null;

    public async Task<StoredImage> Save(ImageContainer container, string objectKey, string contentType, Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        _objects[Full(container, objectKey)] = buffer.ToArray();

        return new StoredImage(
            objectKey,
            container == ImageContainer.BusinessPhotos ? new Uri($"https://pruebas.blob.core.windows.net/business-photos/{objectKey}") : null);
    }

    public Task Delete(ImageContainer container, string objectKey, CancellationToken cancellationToken)
    {
        var full = Full(container, objectKey);
        Deleted.Add(full);
        _objects.TryRemove(full, out _);
        return Task.CompletedTask;
    }

    public Task<TemporaryImageUrl?> CreateTemporaryUrl(ImageContainer container, string objectKey, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        if (!_objects.ContainsKey(Full(container, objectKey)))
        {
            return Task.FromResult<TemporaryImageUrl?>(null);
        }

        var expires = DateTimeOffset.UtcNow.Add(lifetime);
        var url = new Uri($"https://pruebas.blob.core.windows.net/pet-photos/{objectKey}?sig=prueba&se={expires.ToUnixTimeSeconds()}");
        return Task.FromResult<TemporaryImageUrl?>(new TemporaryImageUrl(url, expires));
    }

    private static string Full(ImageContainer container, string objectKey) => $"{container}/{objectKey}";
}
