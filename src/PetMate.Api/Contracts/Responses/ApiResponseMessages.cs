using Microsoft.AspNetCore.Http;

namespace PetMate.Api.Contracts.Responses;

/// <summary>Mensajes estándar de error en español; no contienen detalles internos.</summary>
public static class ApiResponseMessages
{
    public const string InternalError = "Ocurrió un error interno. Inténtelo de nuevo más tarde.";

    public const string ValidationFailed = "La solicitud contiene datos inválidos.";

    public const string InvalidField = "El valor del campo no es válido.";

    public static string ForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "La solicitud no es válida.",
        StatusCodes.Status401Unauthorized => "No autenticado. Inicie sesión para continuar.",
        StatusCodes.Status403Forbidden => "No tiene permiso para realizar esta acción.",
        StatusCodes.Status404NotFound => "Recurso no encontrado.",
        StatusCodes.Status405MethodNotAllowed => "Método HTTP no permitido para este recurso.",
        StatusCodes.Status409Conflict => "La solicitud entra en conflicto con el estado actual del recurso.",
        StatusCodes.Status413PayloadTooLarge => "El cuerpo de la solicitud es demasiado grande.",
        StatusCodes.Status415UnsupportedMediaType => "Tipo de contenido no soportado.",
        StatusCodes.Status429TooManyRequests => "Demasiadas solicitudes. Inténtelo de nuevo más tarde.",
        StatusCodes.Status503ServiceUnavailable => "Servicio no disponible temporalmente.",
        >= 500 => InternalError,
        _ => "No se pudo procesar la solicitud.",
    };
}
