using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrbitShield.Application.Satellites;
using OrbitShield.Domain;

namespace OrbitShield.Api.Controllers;

[ApiController]
[Route("api/satellites")]
public sealed class SatellitesController(ISatelliteService satelliteService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Engineer}")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var satellites = await satelliteService.GetAllAsync(cancellationToken);
        return Ok(satellites);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Engineer}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var satellite = await satelliteService.GetByIdAsync(id, cancellationToken);
        return satellite is null ? NotFound() : Ok(satellite);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create(CreateSatelliteRequest request, CancellationToken cancellationToken)
    {
        var satellite = await satelliteService.CreateAsync(request, cancellationToken);
        return satellite is null ? BadRequest(new { message = "Invalid satellite payload." }) : CreatedAtAction(nameof(GetById), new { id = satellite.Id }, satellite);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(int id, UpdateSatelliteRequest request, CancellationToken cancellationToken)
    {
        var satellite = await satelliteService.UpdateAsync(id, request, cancellationToken);
        return satellite is null ? NotFound(new { message = "Satellite not found or invalid payload." }) : Ok(satellite);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await satelliteService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
