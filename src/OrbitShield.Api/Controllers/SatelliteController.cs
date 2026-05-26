using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrbitShield.Application.Mission;
using OrbitShield.Domain;

namespace OrbitShield.Api.Controllers;

[ApiController]
[Route("api/satellite")]
public sealed class SatelliteController(IMissionControlService missionControl) : ControllerBase
{
    [HttpGet("telemetry")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTelemetry(CancellationToken cancellationToken)
    {
        var telemetry = await missionControl.GetTelemetryAsync(cancellationToken);
        return telemetry is null ? NotFound(new { message = "No satellite registered." }) : Ok(telemetry);
    }

    [HttpGet("conjunctions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConjunctions([FromQuery] bool simulateEmergency = false, CancellationToken cancellationToken = default)
    {
        var conjunction = await missionControl.GetConjunctionAsync(simulateEmergency, cancellationToken);
        return Ok(conjunction);
    }

    [HttpPost("maneuver")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterManeuver(ManeuverRequest request, CancellationToken cancellationToken)
    {
        var response = await missionControl.RegisterManeuverAsync(request, cancellationToken);
        return response.Accepted ? Ok(response) : BadRequest(response);
    }

    [HttpGet("maneuvers")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Engineer}")]
    public async Task<IActionResult> GetRecentManeuvers([FromQuery] int satelliteId = 1, CancellationToken cancellationToken = default)
    {
        var response = await missionControl.GetRecentManeuversAsync(satelliteId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("sensor-readings")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterSensorReading(SensorReadingRequest request, CancellationToken cancellationToken)
    {
        var response = await missionControl.RegisterSensorReadingAsync(request, cancellationToken);
        return Created("/api/satellite/sensor-readings/latest", response);
    }

    [HttpGet("sensor-readings/latest")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLatestSensorReading([FromQuery] int satelliteId = 1, CancellationToken cancellationToken = default)
    {
        var response = await missionControl.GetLatestSensorReadingAsync(satelliteId, cancellationToken);
        return response is null ? NotFound(new { message = "No sensor reading found." }) : Ok(response);
    }

    [HttpPost("orbital-elements")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Engineer}")]
    public async Task<IActionResult> RegisterOrbitalElement(OrbitalElementRequest request, CancellationToken cancellationToken)
    {
        var response = await missionControl.RegisterOrbitalElementAsync(request, cancellationToken);
        return response is null ? BadRequest(new { message = "Invalid TLE payload." }) : Created("/api/satellite/orbital-elements", response);
    }

    [HttpGet("orbital-elements")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Engineer}")]
    public async Task<IActionResult> GetOrbitalElements([FromQuery] int satelliteId = 1, CancellationToken cancellationToken = default)
    {
        var response = await missionControl.GetOrbitalElementsAsync(satelliteId, cancellationToken);
        return Ok(response);
    }
}
