using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PetMate.Api.Middleware;

/// <summary>
/// Correlación y registro de solicitudes (pm-054). Asigna un identificador de correlación a cada solicitud, lo devuelve en
/// <c>X-Correlation-Id</c>, lo incluye como ámbito de todos los logs de la solicitud y registra un único evento estructurado
/// por solicitud con método, plantilla de ruta, código HTTP y duración. Nunca registra cuerpos, cabeceras, consulta, IP,
/// valores de ruta ni identidad del usuario: solo datos técnicos sin PII.
/// </summary>
public sealed partial class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";

    private const string UnknownRoute = "sin-ruta";

    private static readonly Func<ILogger, string, IDisposable?> CorrelationScope = LoggerMessage.DefineScope<string>("CorrelationId {CorrelationId}");

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ResolveCorrelationId(context.Request.Headers[HeaderName].ToString());
        // Se escribe al iniciar la respuesta: el Response Wrapper limpia las cabeceras antes de serializar el cuerpo.
        context.Response.OnStarting(
            static state =>
            {
                var (response, id) = ((HttpResponse, string))state;
                response.Headers[HeaderName] = id;
                return Task.CompletedTask;
            },
            (context.Response, correlationId));
        context.TraceIdentifier = correlationId;

        using var scope = CorrelationScope(logger, correlationId);
        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            var route = RouteOf(context);
            var elapsedMs = Math.Round(Stopwatch.GetElapsedTime(started).TotalMilliseconds, 1);
            LogRequestCompleted(logger, context.Request.Method, route, context.Response.StatusCode, elapsedMs);
        }
    }

    /// <summary>
    /// Acepta el identificador del cliente solo si es corto y seguro (letras, números y guiones); en otro caso genera uno nuevo.
    /// Evita la inyección de saltos de línea o contenido arbitrario en los logs.
    /// </summary>
    public static string ResolveCorrelationId(string? supplied)
        => !string.IsNullOrEmpty(supplied) && ValidCorrelationId().IsMatch(supplied)
            ? supplied
            : Guid.NewGuid().ToString("N");

    // La plantilla de ruta (p. ej. /api/v1/pets/{id}) no contiene identificadores reales ni datos personales.
    private static string RouteOf(HttpContext context)
        => (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? UnknownRoute;

    [GeneratedRegex("^[A-Za-z0-9-]{8,64}$")]
    private static partial Regex ValidCorrelationId();

    [LoggerMessage(Level = LogLevel.Information, Message = "Solicitud completada {Method} {Route} {StatusCode} en {ElapsedMs} ms.")]
    private static partial void LogRequestCompleted(ILogger logger, string method, string route, int statusCode, double elapsedMs);
}
