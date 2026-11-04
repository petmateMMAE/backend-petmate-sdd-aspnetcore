using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetMate.Api.Contracts.Responses;

namespace PetMate.Api.Middleware;

/// <summary>Adapta las respuestas automáticas de MVC (400 de binding y códigos sin cuerpo) al Response Wrapper.</summary>
public static class ApiBehaviorConfiguration
{
    public static void Configure(ApiBehaviorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Evita ProblemDetails en 4xx sin cuerpo: UseStatusCodePages los envuelve en el Response Wrapper.
        options.SuppressMapClientErrors = true;

        options.InvalidModelStateResponseFactory = context =>
        {
            // Mensajes genéricos en español: los textos del framework pueden revelar tipos internos.
            var errors = ApiResponseFactory.BoundErrors(context.ModelState
                .Where(entry => entry.Value is { Errors.Count: > 0 })
                .Select(entry => (
                    string.IsNullOrEmpty(entry.Key) ? "request" : entry.Key,
                    ApiResponseMessages.InvalidField)));
            return ApiResponseFactory.BadRequest(errors);
        };
    }

    public static Task WriteStatusCodeAsync(StatusCodeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var statusCode = context.HttpContext.Response.StatusCode;
        return ApiResponseFactory.WriteAsync<object?>(
            context.HttpContext,
            statusCode,
            ApiResponseMessages.ForStatusCode(statusCode),
            null);
    }
}
