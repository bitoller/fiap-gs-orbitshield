using Microsoft.EntityFrameworkCore;
using OrbitShield.Application.Repositories;
using OrbitShield.Domain.Entities;
using OrbitShield.Infrastructure.Persistence;

namespace OrbitShield.Infrastructure.Repositories;

public sealed class MissionRepository(OrbitShieldDbContext dbContext) : IMissionRepository
{
    public Task<ConjunctionEvent?> GetLatestConjunctionAsync(int satelliteId, CancellationToken cancellationToken = default) =>
        dbContext.ConjunctionEvents
            .Where(x => x.SatelliteId == satelliteId)
            .OrderByDescending(x => x.DetectedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<SensorReading?> GetLatestSensorReadingAsync(int satelliteId, CancellationToken cancellationToken = default) =>
        dbContext.SensorReadings
            .AsNoTracking()
            .Where(x => x.SatelliteId == satelliteId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddConjunctionAsync(ConjunctionEvent conjunctionEvent, CancellationToken cancellationToken = default) =>
        await dbContext.ConjunctionEvents.AddAsync(conjunctionEvent, cancellationToken);

    public async Task AddDebrisObjectAsync(DebrisObject debrisObject, CancellationToken cancellationToken = default) =>
        await dbContext.DebrisObjects.AddAsync(debrisObject, cancellationToken);

    public Task<DebrisObject?> GetDebrisByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        dbContext.DebrisObjects.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public async Task AddManeuverAsync(ManeuverLog maneuverLog, CancellationToken cancellationToken = default) =>
        await dbContext.ManeuverLogs.AddAsync(maneuverLog, cancellationToken);

    public async Task AddSensorReadingAsync(SensorReading sensorReading, CancellationToken cancellationToken = default) =>
        await dbContext.SensorReadings.AddAsync(sensorReading, cancellationToken);

    public async Task AddOrbitalElementAsync(OrbitalElement orbitalElement, CancellationToken cancellationToken = default) =>
        await dbContext.OrbitalElements.AddAsync(orbitalElement, cancellationToken);

    public async Task<IReadOnlyList<ManeuverLog>> GetRecentManeuversAsync(int satelliteId, CancellationToken cancellationToken = default) =>
        await dbContext.ManeuverLogs
            .AsNoTracking()
            .Where(x => x.SatelliteId == satelliteId)
            .OrderByDescending(x => x.ExecutedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OrbitalElement>> GetOrbitalElementsAsync(int satelliteId, CancellationToken cancellationToken = default) =>
        await dbContext.OrbitalElements
            .AsNoTracking()
            .Where(x => x.SatelliteId == satelliteId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}
