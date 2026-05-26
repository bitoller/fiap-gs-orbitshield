namespace OrbitShield.Application.Mission;

public sealed record TelemetryResponse(
    int SatelliteId,
    string SatelliteCode,
    int Fuel,
    int SolarEnergy,
    string OrbitStatus);

public sealed record ConjunctionResponse(
    bool CollisionRisk,
    decimal Distance,
    decimal Probability,
    string DebrisId,
    string Status);

public sealed record ManeuverRequest(
    int SatelliteId,
    int? ConjunctionEventId,
    string Action,
    int ServoAngle,
    decimal? ThrustLevel,
    string Source);

public sealed record ManeuverResponse(bool Accepted, string Message);

public sealed record ManeuverLogResponse(
    int Id,
    int SatelliteId,
    int? ConjunctionEventId,
    string Action,
    int ServoAngle,
    decimal? ThrustLevel,
    string Source,
    DateTime ExecutedAt);

public sealed record SensorReadingRequest(
    int SatelliteId,
    decimal TemperatureCelsius,
    decimal RadiationLevel,
    decimal BatteryVoltage,
    decimal? SimulatedGravity,
    decimal? SimulatedThrust);

public sealed record SensorReadingResponse(
    int Id,
    int SatelliteId,
    decimal TemperatureCelsius,
    decimal RadiationLevel,
    decimal BatteryVoltage,
    decimal? SimulatedGravity,
    decimal? SimulatedThrust,
    DateTime CreatedAt);

public sealed record OrbitalElementRequest(
    int SatelliteId,
    string Name,
    string TleLine1,
    string TleLine2,
    DateTime Epoch,
    string Source);

public sealed record OrbitalElementResponse(
    int Id,
    int SatelliteId,
    string Name,
    string TleLine1,
    string TleLine2,
    DateTime Epoch,
    string Source,
    DateTime CreatedAt);
