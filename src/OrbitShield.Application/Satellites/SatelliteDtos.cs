namespace OrbitShield.Application.Satellites;

public sealed record SatelliteResponse(
    int Id,
    string Code,
    string Name,
    string OrbitStatus,
    int FuelPercentage,
    int SolarEnergyPercentage,
    DateTime CreatedAt);

public sealed record CreateSatelliteRequest(
    string Code,
    string Name,
    string OrbitStatus,
    int FuelPercentage,
    int SolarEnergyPercentage);

public sealed record UpdateSatelliteRequest(
    string Name,
    string OrbitStatus,
    int FuelPercentage,
    int SolarEnergyPercentage);
