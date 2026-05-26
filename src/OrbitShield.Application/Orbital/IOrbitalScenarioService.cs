namespace OrbitShield.Application.Orbital;

public interface IOrbitalScenarioService
{
    Task<OrbitalEnvironmentResponse?> SpawnDebrisAsync(SpawnDebrisRequest request, CancellationToken cancellationToken = default);
    Task<OrbitalEnvironmentResponse?> SpawnRandomDebrisAsync(RandomDebrisRequest request, CancellationToken cancellationToken = default);
    Task<OrbitalEnvironmentResponse?> SpawnPresetAsync(int satelliteId, OrbitalScenarioPreset preset, CancellationToken cancellationToken = default);
    Task<OrbitalEnvironmentResponse?> GetEnvironmentAsync(int satelliteId, CancellationToken cancellationToken = default);
    IReadOnlyCollection<OrbitalScenarioPresetResponse> ListPresets(int satelliteId);
}
