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
    decimal EffectiveCrossSectionM2,
    decimal? DiameterMeters = null,
    decimal? EstimatedMassKg = null);

public sealed record RandomDebrisRequest(
    int SatelliteId,
    decimal SafeDistanceKm = 5,
    decimal MinimumDiameterMeters = 0.05m,
    decimal MaximumDiameterMeters = 8.0m);

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
    decimal DebrisDiameterMeters,
    decimal EstimatedMassKg,
    decimal ImpactEnergyJoules,
    string DebrisClass,
    string ScenarioClassification,
    decimal CollisionProbability,
    string ProbabilityModel,
    bool RecommendedEmergency,
    bool PredictedImpact);
