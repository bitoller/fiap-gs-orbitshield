using OrbitShield.Application.Abstractions;
using OrbitShield.Application.Repositories;
using OrbitShield.Domain;
using OrbitShield.Domain.Entities;

namespace OrbitShield.Application.Mission;

public sealed class MissionControlService(
    ISatelliteRepository satellites,
    IMissionRepository mission,
    IUnitOfWork unitOfWork) : IMissionControlService
{
    private const string EmergencyDebrisCode = "DEBRIS-2026";

    public async Task<TelemetryResponse?> GetTelemetryAsync(CancellationToken cancellationToken = default)
    {
        var satellite = await satellites.GetDefaultAsync(cancellationToken);
        return satellite is null
            ? null
            : new TelemetryResponse(
                satellite.Id,
                satellite.Code,
                satellite.FuelPercentage,
                satellite.SolarEnergyPercentage,
                satellite.OrbitStatus);
    }

    public async Task<ConjunctionResponse> GetConjunctionAsync(bool simulateEmergency, CancellationToken cancellationToken = default)
    {
        if (!simulateEmergency)
        {
            return new ConjunctionResponse(false, 1240, 0.7m, "NONE", "Nominal");
        }

        var satellite = await satellites.GetDefaultAsync(cancellationToken);
        if (satellite is not null)
        {
            satellite.OrbitStatus = OrbitStatuses.Emergency;

            var debris = await mission.GetDebrisByCodeAsync(EmergencyDebrisCode, cancellationToken)
                ?? new DebrisObject
                {
                    Code = EmergencyDebrisCode,
                    EstimatedSizeMeters = 1.7m,
                    RiskLevel = RiskLevels.Critical
                };

            if (debris.Id == 0)
            {
                await mission.AddDebrisObjectAsync(debris, cancellationToken);
            }

            await mission.AddConjunctionAsync(new ConjunctionEvent
            {
                SatelliteId = satellite.Id,
                DebrisObject = debris,
                DistanceKm = 12,
                Probability = 98.5m,
                CollisionRisk = true,
                Status = ConjunctionStatuses.Open
            }, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ConjunctionResponse(true, 12, 98.5m, EmergencyDebrisCode, "Critical");
    }

    public async Task<ManeuverResponse> RegisterManeuverAsync(ManeuverRequest request, CancellationToken cancellationToken = default)
    {
        var satellite = await satellites.GetByIdAsync(request.SatelliteId, cancellationToken);
        if (satellite is null)
        {
            return new ManeuverResponse(false, "Satellite not found.");
        }

        satellite.OrbitStatus = OrbitStatuses.Warning;
        var latestConjunction = request.ConjunctionEventId.HasValue
            ? null
            : await mission.GetLatestConjunctionAsync(request.SatelliteId, cancellationToken);

        await mission.AddManeuverAsync(new ManeuverLog
        {
            SatelliteId = request.SatelliteId,
            ConjunctionEventId = request.ConjunctionEventId ?? latestConjunction?.Id,
            Action = request.Action.Trim(),
            ServoAngle = request.ServoAngle,
            ThrustLevel = request.ThrustLevel,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "ESP32" : request.Source.Trim()
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ManeuverResponse(true, "Maneuver log registered by Mission Control.");
    }

    public async Task<IReadOnlyList<ManeuverLogResponse>> GetRecentManeuversAsync(int satelliteId, CancellationToken cancellationToken = default)
    {
        var maneuvers = await mission.GetRecentManeuversAsync(satelliteId, cancellationToken);
        return maneuvers.Select(x => new ManeuverLogResponse(
            x.Id,
            x.SatelliteId,
            x.ConjunctionEventId,
            x.Action,
            x.ServoAngle,
            x.ThrustLevel,
            x.Source,
            x.ExecutedAt)).ToArray();
    }

    public async Task<SensorReadingResponse> RegisterSensorReadingAsync(SensorReadingRequest request, CancellationToken cancellationToken = default)
    {
        var reading = new SensorReading
        {
            SatelliteId = request.SatelliteId,
            TemperatureCelsius = request.TemperatureCelsius,
            RadiationLevel = request.RadiationLevel,
            BatteryVoltage = request.BatteryVoltage,
            SimulatedGravity = request.SimulatedGravity,
            SimulatedThrust = request.SimulatedThrust
        };

        await mission.AddSensorReadingAsync(reading, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(reading);
    }

    public async Task<SensorReadingResponse?> GetLatestSensorReadingAsync(int satelliteId, CancellationToken cancellationToken = default)
    {
        var reading = await mission.GetLatestSensorReadingAsync(satelliteId, cancellationToken);
        return reading is null ? null : Map(reading);
    }

    public async Task<OrbitalElementResponse?> RegisterOrbitalElementAsync(OrbitalElementRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsValidTle(request.TleLine1, request.TleLine2))
        {
            return null;
        }

        var satellite = await satellites.GetByIdAsync(request.SatelliteId, cancellationToken);
        if (satellite is null)
        {
            return null;
        }

        var orbitalElement = new OrbitalElement
        {
            SatelliteId = request.SatelliteId,
            Name = request.Name.Trim(),
            TleLine1 = request.TleLine1.Trim(),
            TleLine2 = request.TleLine2.Trim(),
            Epoch = request.Epoch,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "CelesTrak" : request.Source.Trim()
        };

        await mission.AddOrbitalElementAsync(orbitalElement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrbitalElementResponse(
            orbitalElement.Id,
            orbitalElement.SatelliteId,
            orbitalElement.Name,
            orbitalElement.TleLine1,
            orbitalElement.TleLine2,
            orbitalElement.Epoch,
            orbitalElement.Source,
            orbitalElement.CreatedAt);
    }

    public async Task<IReadOnlyList<OrbitalElementResponse>> GetOrbitalElementsAsync(int satelliteId, CancellationToken cancellationToken = default)
    {
        var orbitalElements = await mission.GetOrbitalElementsAsync(satelliteId, cancellationToken);
        return orbitalElements.Select(x => new OrbitalElementResponse(
            x.Id,
            x.SatelliteId,
            x.Name,
            x.TleLine1,
            x.TleLine2,
            x.Epoch,
            x.Source,
            x.CreatedAt)).ToArray();
    }

    private static SensorReadingResponse Map(SensorReading reading) =>
        new(
            reading.Id,
            reading.SatelliteId,
            reading.TemperatureCelsius,
            reading.RadiationLevel,
            reading.BatteryVoltage,
            reading.SimulatedGravity,
            reading.SimulatedThrust,
            reading.CreatedAt);

    private static bool IsValidTle(string line1, string line2) =>
        !string.IsNullOrWhiteSpace(line1)
        && !string.IsNullOrWhiteSpace(line2)
        && line1.TrimStart().StartsWith('1')
        && line2.TrimStart().StartsWith('2')
        && line1.Trim().Length >= 60
        && line2.Trim().Length >= 60;
}
