namespace OrbitShield.Application.Orbital;

public enum OrbitalScenarioPreset
{
    SafePass,
    NearMiss,
    CriticalImpact,
    LateDetection,
    DenseDebrisField
}

public sealed record OrbitalVectorResponse(decimal X, decimal Y, decimal Z);

public sealed record SpawnDebrisRequest(
    int SatelliteId,
    decimal ApproachTimeSeconds,
    decimal MissDistanceKm,
    decimal SafeDistanceKm,
    decimal DebrisDensityPerKm3,
    decimal EffectiveCrossSectionM2);

public sealed record OrbitalScenarioPresetResponse(
    string Name,
    string Description,
    SpawnDebrisRequest Request);

public sealed record OrbitalEnvironmentResponse(
    int SatelliteId,
    string SatelliteCode,
    string Source,
    string TleName,
    string TleLine1,
    string TleLine2,
    DateTime EpochUtc,
    OrbitalVectorResponse SatellitePositionKm,
    OrbitalVectorResponse SatelliteVelocityKmS,
    OrbitalVectorResponse RelativePositionKm,
    OrbitalVectorResponse RelativeVelocityKmS,
    decimal SafeDistanceKm,
    decimal LookaheadSeconds,
    decimal TimeToClosestApproachSeconds,
    decimal MissDistanceKm,
    decimal RelativeSpeedKmS,
    decimal CollisionProbability,
    string ProbabilityModel,
    bool RecommendedEmergency);
