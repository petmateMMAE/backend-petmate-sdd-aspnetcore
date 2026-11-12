using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Images;

namespace PetMate.Api.Contracts.Images;

/// <summary>Foto privada de una mascota: URL temporal y cuándo deja de servir (pm-081).</summary>
public sealed record PetPhotoResponse(Guid PetId, string Url, DateTimeOffset ExpiresAt);

/// <summary>Foto pública del negocio (pm-111).</summary>
public sealed record BusinessPhotoResponse(string PhotoUrl);

/// <summary>Respuestas comunes de las rutas de imágenes.</summary>
public static class PhotoResponses
{
    /// <summary>Mensaje cuando el almacenamiento no está configurado en el entorno.</summary>
    public const string StorageUnavailable = "El almacenamiento de imágenes no está disponible.";

    /// <summary>Traduce el rechazo a una respuesta; devuelve <c>null</c> si la imagen es válida.</summary>
    public static ObjectResult? Rejected(ImageRejection rejection, ImageContainer container) => rejection switch
    {
        ImageRejection.None => null,
        ImageRejection.TooLarge => ApiResponseFactory.Create<object?>(
            StatusCodes.Status413PayloadTooLarge,
            $"La imagen supera el máximo de {ImageRules.MaxBytes(container) / (1024 * 1024)} MB.",
            null),
        ImageRejection.UnsupportedType => ApiResponseFactory.Create<object?>(
            StatusCodes.Status400BadRequest,
            $"El archivo debe ser una imagen {string.Join(", ", ImageRules.AllowedContentTypes)}.",
            null),
        _ => ApiResponseFactory.Create<object?>(
            StatusCodes.Status400BadRequest,
            $"Falta el archivo: se espera un cuerpo multipart/form-data con el campo «{PhotoUploadReader.FieldName}».",
            null),
    };

    /// <summary>503 cuando el entorno no tiene almacenamiento de imágenes configurado.</summary>
    public static ObjectResult Unavailable()
        => ApiResponseFactory.Create<object?>(StatusCodes.Status503ServiceUnavailable, StorageUnavailable, null);
}
