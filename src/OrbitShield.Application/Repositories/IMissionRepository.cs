using OrbitShield.Domain.Entities;

namespace OrbitShield.Application.Repositories;

public interface IMissionRepository
{
    Task<ConjunctionEvent?> GetLatestConjunctionAsync(int satelliteId, CancellationToken cancellationToken = default);
    Task<SensorReading?> GetLatestSensorReadingAsync(int satelliteId, CancellationToken cancellationToken = default);
    Task AddConjunctionAsync(ConjunctionEvent conjunctionEvent, CancellationToken cancellationToken = default);
    Task AddDebrisObjectAsync(DebrisObject debrisObject, CancellationToken cancellationToken = default);
    Task<DebrisObject?> GetDebrisByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddManeuverAsync(ManeuverLog maneuverLog, CancellationToken cancellationToken = default);
    Task AddSensorReadingAsync(SensorReading sensorReading, CancellationToken cancellationToken = default);
    Task AddOrbitalElementAsync(OrbitalElement orbitalElement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManeuverLog>> GetRecentManeuversAsync(int satelliteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrbitalElement>> GetOrbitalElementsAsync(int satelliteId, CancellationToken cancellationToken = default);
}
