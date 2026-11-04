using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PetMate.Api.Contracts.Responses;

/// <summary>Punto único de creación y escritura del Response Wrapper (controladores, middleware y status pages).</summary>
public static class ApiResponseFactory
{
    public static ObjectResult Create<T>(int statusCode, string message, T? data)
        => new(new ApiResponse<T>(statusCode, message, data))
        {
            StatusCode = statusCode,
        };

    public static ObjectResult Ok<T>(string message, T? data)
        => Create(StatusCodes.Status200OK, message, data);

    public static ObjectResult InternalServerError()
        => Create<object?>(
            StatusCodes.Status500InternalServerError,
            ApiResponseMessages.InternalError,
            null);

    public static ObjectResult BadRequest(IReadOnlyList<ValidationErrorItem> errors)
        => Create(
            StatusCodes.Status400BadRequest,
            ApiResponseMessages.ValidationFailed,
            new ValidationErrorsData(errors));

    public const int MaxErrors = 50;

    public const int MaxTextLength = 200;

    /// <summary>Acota cantidad y longitud de los errores para evitar amplificación de la respuesta.</summary>
    public static ValidationErrorItem[] BoundErrors(IEnumerable<(string Field, string Message)> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return [.. errors
            .Take(MaxErrors)
            .Select(error => new ValidationErrorItem(Truncate(error.Field), Truncate(error.Message)))];
    }

    private static string Truncate(string? value)
        => value is { Length: > MaxTextLength } ? value[..MaxTextLength] : value ?? string.Empty;

    /// <summary>Escribe el wrapper en la respuesta HTTP actual (usado fuera de MVC).</summary>
    public static async Task WriteAsync<T>(
        HttpContext context,
        int statusCode,
        string message,
        T? data,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        foreach (var (name, value) in headers ?? new Dictionary<string, string>())
        {
            context.Response.Headers[name] = value;
        }

        await context.Response.WriteAsJsonAsync(
            new ApiResponse<T>(statusCode, message, data),
            ApiJson.Options,
            "application/json; charset=utf-8");
    }
}
