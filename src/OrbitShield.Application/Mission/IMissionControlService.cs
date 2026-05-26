namespace OrbitShield.Application.Mission;

public interface IMissionControlService
{
    Task<TelemetryResponse?> GetTelemetryAsync(CancellationToken cancellationToken = default);
    Task<ConjunctionResponse> GetConjunctionAsync(bool simulateEmergency, CancellationToken cancellationToken = default);
    Task<ManeuverResponse> RegisterManeuverAsync(ManeuverRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManeuverLogResponse>> GetRecentManeuversAsync(int satelliteId, CancellationToken cancellationToken = default);
    Task<SensorReadingResponse> RegisterSensorReadingAsync(SensorReadingRequest request, CancellationToken cancellationToken = default);
    Task<SensorReadingResponse?> GetLatestSensorReadingAsync(int satelliteId, CancellationToken cancellationToken = default);
    Task<OrbitalElementResponse?> RegisterOrbitalElementAsync(OrbitalElementRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrbitalElementResponse>> GetOrbitalElementsAsync(int satelliteId, CancellationToken cancellationToken = default);
}
