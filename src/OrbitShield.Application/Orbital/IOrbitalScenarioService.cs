namespace OrbitShield.Application.Orbital;

public interface IOrbitalScenarioService
{
    Task<OrbitalEnvironmentResponse?> SpawnDebrisAsync(SpawnDebrisRequest request, CancellationToken cancellationToken = default);
    Task<OrbitalEnvironmentResponse?> GetEnvironmentAsync(int satelliteId, CancellationToken cancellationToken = default);
}
