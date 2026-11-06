using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Mediator;

namespace PetMate.Api.Controllers;

/// <summary>Controlador MVC técnico de prueba; no expone una ruta de negocio.</summary>
[ApiController]
// Público de forma temporal para no ampliar el alcance de pm-033. Pendiente (decisión P-04): revisar su exposición en producción
// en un trabajo de seguridad posterior (restringir a Development o protegerlo sin crear un rol de administración).
[AllowAnonymous]
[Route("api/v1/technical")]
public sealed class TechnicalMediatorProbeController(ISender sender) : ControllerBase
{
    // Ruta técnica versionada para validar las convenciones MVC y OpenAPI de T007.
    [HttpGet("mediator-probe")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> Execute(CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new TechnicalProbeQuery(), cancellationToken);
        return ApiResponseFactory.Ok("Consulta técnica procesada correctamente.", result);
    }
}
