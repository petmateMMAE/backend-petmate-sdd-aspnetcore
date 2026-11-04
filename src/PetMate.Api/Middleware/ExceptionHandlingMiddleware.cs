using Microsoft.AspNetCore.Http;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Exceptions;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Application.Validation;

namespace PetMate.Api.Middleware;

/// <summary>
/// Manejo centralizado de excepciones: traduce cada excepción al Response Wrapper sin exponer
/// mensajes, tipos ni trazas de errores inesperados. El detalle solo se registra en el log del servidor.
/// </summary>
public sealed partial class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente canceló la solicitud: no hay a quién responder.
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        switch (exception)
        {
            case RequestValidationException validation:
                await ApiResponseFactory.WriteAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    ApiResponseMessages.ValidationFailed,
                    new ValidationErrorsData(ApiResponseFactory.BoundErrors(
                        validation.Failures.Select(failure => (failure.Field, failure.Message)))));
                break;
            case PersistenceUnavailableException persistence:
                LogPersistenceUnavailable(logger, persistence.InnerException?.GetType().Name ?? "desconocido");
                await ApiResponseFactory.WriteAsync<object?>(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    ApiResponseMessages.ForStatusCode(StatusCodes.Status503ServiceUnavailable),
                    null);
                break;
            case AuthDependencyUnavailableException authDependency:
                LogAuthDependencyUnavailable(logger, authDependency.InnerException?.GetType().Name ?? "desconocido");
                await ApiResponseFactory.WriteAsync<object?>(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    ApiResponseMessages.ForStatusCode(StatusCodes.Status503ServiceUnavailable),
                    null);
                break;
            case BadHttpRequestException badRequest:
                // Error de cliente (cuerpo excedido, cabecera inválida): no se usa el mensaje del framework.
                await ApiResponseFactory.WriteAsync<object?>(
                    context,
                    badRequest.StatusCode,
                    ApiResponseMessages.ForStatusCode(badRequest.StatusCode),
                    null);
                break;
            case ApiException api:
                await ApiResponseFactory.WriteAsync<object?>(context, api.StatusCode, api.Message, null);
                break;
            default:
                LogUnexpectedException(logger, exception);
                await ApiResponseFactory.WriteAsync<object?>(
                    context,
                    StatusCodes.Status500InternalServerError,
                    ApiResponseMessages.InternalError,
                    null);
                break;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Dependencia de seguridad no disponible ({ErrorType}).")]
    private static partial void LogAuthDependencyUnavailable(ILogger logger, string errorType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Persistencia no disponible ({ErrorType}).")]
    private static partial void LogPersistenceUnavailable(ILogger logger, string errorType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Excepción no controlada durante la solicitud.")]
    private static partial void LogUnexpectedException(ILogger logger, Exception exception);
}
