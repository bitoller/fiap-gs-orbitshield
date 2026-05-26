using OrbitShield.Application.Abstractions;
using OrbitShield.Application.Common;
using OrbitShield.Application.Repositories;
using OrbitShield.Domain;
using OrbitShield.Domain.Entities;

namespace OrbitShield.Application.Satellites;

public sealed class SatelliteService(
    ISatelliteRepository satellites,
    IUnitOfWork unitOfWork) : ISatelliteService
{
    private static readonly HashSet<string> AllowedStatuses = [OrbitStatuses.Nominal, OrbitStatuses.Warning, OrbitStatuses.Emergency];

    public async Task<IReadOnlyList<SatelliteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await satellites.GetAllAsync(cancellationToken);
        return records.Select(Map).ToArray();
    }

    public async Task<SatelliteResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var satellite = await satellites.GetByIdAsync(id, cancellationToken);
        return satellite is null ? null : Map(satellite);
    }

    public async Task<SatelliteResponse?> CreateAsync(CreateSatelliteRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsValidPercent(request.FuelPercentage) || !IsValidPercent(request.SolarEnergyPercentage))
        {
            return null;
        }

        if (!AllowedStatuses.Contains(request.OrbitStatus))
        {
            return null;
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await satellites.ExistsByCodeAsync(code, cancellationToken))
        {
            return null;
        }

        var satellite = new Satellite
        {
            Code = code,
            Name = request.Name.Trim(),
            OrbitStatus = request.OrbitStatus,
            FuelPercentage = request.FuelPercentage,
            SolarEnergyPercentage = request.SolarEnergyPercentage
        };

        await satellites.AddAsync(satellite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(satellite);
    }

    public async Task<SatelliteResponse?> UpdateAsync(int id, UpdateSatelliteRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsValidPercent(request.FuelPercentage) || !IsValidPercent(request.SolarEnergyPercentage))
        {
            return null;
        }

        if (!AllowedStatuses.Contains(request.OrbitStatus))
        {
            return null;
        }

        var satellite = await satellites.GetByIdAsync(id, cancellationToken);
        if (satellite is null)
        {
            return null;
        }

        satellite.Name = request.Name.Trim();
        satellite.OrbitStatus = request.OrbitStatus;
        satellite.FuelPercentage = request.FuelPercentage;
        satellite.SolarEnergyPercentage = request.SolarEnergyPercentage;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(satellite);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var satellite = await satellites.GetByIdAsync(id, cancellationToken);
        if (satellite is null)
        {
            return OperationResult.Fail("Satellite not found.");
        }

        satellites.Remove(satellite);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Satellite deleted successfully.");
    }

    private static SatelliteResponse Map(Satellite satellite) =>
        new(
            satellite.Id,
            satellite.Code,
            satellite.Name,
            satellite.OrbitStatus,
            satellite.FuelPercentage,
            satellite.SolarEnergyPercentage,
            satellite.CreatedAt);

    private static bool IsValidPercent(int value) => value is >= 0 and <= 100;
}
