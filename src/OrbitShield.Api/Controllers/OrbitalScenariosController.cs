using Microsoft.AspNetCore.Mvc;
using OrbitShield.Application.Orbital;

namespace OrbitShield.Api.Controllers;

[ApiController]
[Route("api/orbital-scenarios")]
public sealed class OrbitalScenariosController(IOrbitalScenarioService orbitalScenarios) : ControllerBase
{
    [HttpPost("satellites/{satelliteId:int}/spawn-debris")]
    public async Task<IActionResult> SpawnDebris(int satelliteId, SpawnDebrisRequest request, CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { SatelliteId = satelliteId };
        var response = await orbitalScenarios.SpawnDebrisAsync(normalizedRequest, cancellationToken);
        return response is null ? NotFound(new { message = "Satellite not found." }) : Ok(response);
    }

    [HttpGet("satellites/{satelliteId:int}/environment")]
    public async Task<IActionResult> GetEnvironment(int satelliteId, CancellationToken cancellationToken)
    {
        var response = await orbitalScenarios.GetEnvironmentAsync(satelliteId, cancellationToken);
        return response is null ? NotFound(new { message = "Satellite not found." }) : Ok(response);
    }
}
