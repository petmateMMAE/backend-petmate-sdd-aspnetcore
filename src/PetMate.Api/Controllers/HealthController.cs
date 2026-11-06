using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetMate.Api.Contracts.Responses;
using PetMate.Infrastructure.Health;

namespace PetMate.Api.Controllers;

/// <summary>Estado técnico de la API y sus dependencias; no expone detalles internos ni secretos.</summary>
[ApiController]
[AllowAnonymous] // Público a propósito: lo consultan las sondas del balanceador y de Compose (pm-033).
[Route("api/v1/health")]
#pragma warning disable CA1822 // MVC no enruta acciones static.
public sealed class HealthController(HealthCheckService healthChecks) : ControllerBase
{
    /// <summary>Liveness: el proceso está vivo. No consulta dependencias.</summary>
    [HttpGet("live")]
    [ProducesResponseType(typeof(ApiResponse<HealthData>), StatusCodes.Status200OK)]
    public ObjectResult Live()
        => ApiResponseFactory.Ok("La API está en ejecución.", new HealthData("Healthy", []));

    /// <summary>Readiness: MongoDB, Redis Auth y Redis Cache responden. 503 si alguno falla.</summary>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ApiResponse<HealthData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HealthData>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ObjectResult> Ready(CancellationToken cancellationToken = default)
    {
        var report = await healthChecks.CheckHealthAsync(
            registration => registration.Tags.Contains(HealthCheckNames.ReadyTag),
            cancellationToken);

        var dependencies = report.Entries
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new DependencyHealth(entry.Key, entry.Value.Status.ToString()))
            .ToArray();
        var healthy = report.Status == HealthStatus.Healthy;
        var data = new HealthData(healthy ? "Healthy" : "Unhealthy", dependencies);

        return healthy
            ? ApiResponseFactory.Ok("Todas las dependencias están disponibles.", data)
            : ApiResponseFactory.Create(
                StatusCodes.Status503ServiceUnavailable,
                "Una o más dependencias no están disponibles.",
                data);
    }
}

public sealed record HealthData(string Status, IReadOnlyList<DependencyHealth> Dependencies);

public sealed record DependencyHealth(string Name, string Status);
