using OrbitShield.Application.Repositories;
using SGPdotNET.Observation;

namespace OrbitShield.Application.Orbital;

public sealed class OrbitalScenarioService(ISatelliteRepository satellites) : IOrbitalScenarioService
{
    private const int IssCatalogNumber = 25544;
    private const string CelesTrakTleUrl = "https://celestrak.org/NORAD/elements/gp.php?CATNR=25544&FORMAT=TLE";
    private static readonly HttpClient HttpClient = new();
    private static OrbitalEnvironmentResponse? activeScenario;

    public IReadOnlyCollection<OrbitalScenarioPresetResponse> ListPresets(int satelliteId) =>
        Enum.GetValues<OrbitalScenarioPreset>()
            .Select(preset => new OrbitalScenarioPresetResponse(
                preset.ToString(),
                Describe(preset),
                CreatePresetRequest(satelliteId, preset)))
            .ToArray();

    public async Task<OrbitalEnvironmentResponse?> SpawnDebrisAsync(SpawnDebrisRequest request, CancellationToken cancellationToken = default)
    {
        var satellite = await satellites.GetByIdAsync(request.SatelliteId, cancellationToken);
        if (satellite is null)
        {
            return null;
        }

        var tle = await FetchIssTleAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var propagated = new Satellite(tle.Name, tle.Line1, tle.Line2).Predict(now);

        var approachSeconds = Clamp(request.ApproachTimeSeconds, 20, 300);
        var safeDistanceKm = Clamp(request.SafeDistanceKm, 1, 25);
        var missDistanceKm = Clamp(request.MissDistanceKm, 0, safeDistanceKm * 2);

        var relativePosition = new OrbitalVector(12.0m, -4.0m, 1.2m);
        var perpendicularMiss = new OrbitalVector(0.0m, missDistanceKm, 0.0m);
        var relativeVelocity = (perpendicularMiss - relativePosition) / approachSeconds;

        var analysis = Analyze(
            relativePosition,
            relativeVelocity,
            safeDistanceKm,
            approachSeconds * 2,
            request.DebrisDensityPerKm3,
            request.EffectiveCrossSectionM2);

        activeScenario = new OrbitalEnvironmentResponse(
            satellite.Id,
            satellite.Code,
            "CelesTrak TLE + SGP4 + injected debris vector",
            tle.Name,
            tle.Line1,
            tle.Line2,
            now,
            ToResponse(propagated.Position),
            ToResponse(propagated.Velocity),
            ToResponse(relativePosition),
            ToResponse(relativeVelocity),
            safeDistanceKm,
            approachSeconds * 2,
            analysis.TimeToClosestApproachSeconds,
            analysis.MissDistanceKm,
            analysis.RelativeSpeedKmS,
            analysis.CollisionProbability,
            "P = 1 - exp(-(relativeSpeedKmS * effectiveCrossSectionKm2 * debrisDensityPerKm3 * lookaheadSeconds))",
            analysis.MissDistanceKm <= safeDistanceKm || analysis.CollisionProbability >= 50);

        return activeScenario;
    }

    public Task<OrbitalEnvironmentResponse?> SpawnPresetAsync(int satelliteId, OrbitalScenarioPreset preset, CancellationToken cancellationToken = default) =>
        SpawnDebrisAsync(CreatePresetRequest(satelliteId, preset), cancellationToken);

    public async Task<OrbitalEnvironmentResponse?> GetEnvironmentAsync(int satelliteId, CancellationToken cancellationToken = default)
    {
        if (activeScenario is not null && activeScenario.SatelliteId == satelliteId)
        {
            return activeScenario;
        }

        return await SpawnDebrisAsync(new SpawnDebrisRequest(
            satelliteId,
            ApproachTimeSeconds: 90,
            MissDistanceKm: 0.8m,
            SafeDistanceKm: 5,
            DebrisDensityPerKm3: 0.000000015m,
            EffectiveCrossSectionM2: 4000000m), cancellationToken);
    }

    private static OrbitalAnalysis Analyze(
        OrbitalVector relativePosition,
        OrbitalVector relativeVelocity,
        decimal safeDistanceKm,
        decimal lookaheadSeconds,
        decimal debrisDensityPerKm3,
        decimal effectiveCrossSectionM2)
    {
        var velocityNormSquared = relativeVelocity.Dot(relativeVelocity);
        var rawTca = velocityNormSquared <= 0 ? 0 : -relativePosition.Dot(relativeVelocity) / velocityNormSquared;
        var tca = Clamp(rawTca, 0, lookaheadSeconds);
        var closestVector = relativePosition + (relativeVelocity * tca);
        var missDistanceKm = closestVector.Length;
        var relativeSpeedKmS = relativeVelocity.Length;
        var effectiveCrossSectionKm2 = Math.Max(0, (double)effectiveCrossSectionM2) / 1_000_000.0;
        var density = Math.Max(0, (double)debrisDensityPerKm3);
        var hazard = (double)relativeSpeedKmS * effectiveCrossSectionKm2 * density * (double)lookaheadSeconds;
        var probability = (decimal)((1.0 - Math.Exp(-hazard)) * 100.0);

        if (missDistanceKm <= safeDistanceKm)
        {
            probability = Math.Max(probability, 98.5m);
        }

        return new OrbitalAnalysis(tca, missDistanceKm, relativeSpeedKmS, Math.Round(probability, 2));
    }

    private static async Task<CelesTrakTle> FetchIssTleAsync(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(CelesTrakTleUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var line1Index = content.IndexOf("1 ", StringComparison.Ordinal);
        var line2Index = content.IndexOf("2 ", StringComparison.Ordinal);

        if (line1Index < 0 || line2Index < 0)
        {
            throw new InvalidOperationException("CelesTrak TLE response is invalid.");
        }

        var name = content[..line1Index].Trim();
        var line1 = content[line1Index..line2Index].Trim();
        var line2 = content[line2Index..].Trim();

        return new CelesTrakTle(string.IsNullOrWhiteSpace(name) ? "ISS (ZARYA)" : name, line1, line2);
    }

    private static OrbitalVectorResponse ToResponse(SGPdotNET.Util.Vector3 vector) =>
        new((decimal)vector.X, (decimal)vector.Y, (decimal)vector.Z);

    private static OrbitalVectorResponse ToResponse(OrbitalVector vector) =>
        new(vector.X, vector.Y, vector.Z);

    private static decimal Clamp(decimal value, decimal min, decimal max) =>
        Math.Min(Math.Max(value, min), max);

    private static SpawnDebrisRequest CreatePresetRequest(int satelliteId, OrbitalScenarioPreset preset) =>
        preset switch
        {
            OrbitalScenarioPreset.SafePass => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 120,
                MissDistanceKm: 12,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000005m,
                EffectiveCrossSectionM2: 1000000m),

            OrbitalScenarioPreset.NearMiss => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 120,
                MissDistanceKm: 4.8m,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000010m,
                EffectiveCrossSectionM2: 2500000m),

            OrbitalScenarioPreset.CriticalImpact => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 75,
                MissDistanceKm: 0.5m,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000015m,
                EffectiveCrossSectionM2: 4000000m),

            OrbitalScenarioPreset.LateDetection => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 25,
                MissDistanceKm: 1.2m,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000020m,
                EffectiveCrossSectionM2: 4000000m),

            OrbitalScenarioPreset.DenseDebrisField => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 90,
                MissDistanceKm: 3.5m,
                SafeDistanceKm: 6,
                DebrisDensityPerKm3: 0.000000050m,
                EffectiveCrossSectionM2: 6000000m),

            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown orbital scenario preset.")
        };

    private static string Describe(OrbitalScenarioPreset preset) =>
        preset switch
        {
            OrbitalScenarioPreset.SafePass => "Debris passes outside the safety volume. The ESP32 should keep the actuator nominal.",
            OrbitalScenarioPreset.NearMiss => "Debris crosses near the safety boundary. The ESP32 should perform a mild avoidance maneuver.",
            OrbitalScenarioPreset.CriticalImpact => "Debris crosses deep inside the safety volume. The ESP32 should command a strong avoidance maneuver.",
            OrbitalScenarioPreset.LateDetection => "Debris is detected with little reaction time. The ESP32 should command an urgent maneuver.",
            OrbitalScenarioPreset.DenseDebrisField => "Higher debris density and cross-section produce a stronger probability signal.",
            _ => "Unknown scenario."
        };

    private sealed record CelesTrakTle(string Name, string Line1, string Line2);
    private sealed record OrbitalAnalysis(decimal TimeToClosestApproachSeconds, decimal MissDistanceKm, decimal RelativeSpeedKmS, decimal CollisionProbability);

    private readonly record struct OrbitalVector(decimal X, decimal Y, decimal Z)
    {
        public decimal Length => (decimal)Math.Sqrt((double)Dot(this));
        public decimal Dot(OrbitalVector other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);
        public static OrbitalVector operator +(OrbitalVector left, OrbitalVector right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static OrbitalVector operator -(OrbitalVector left, OrbitalVector right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        public static OrbitalVector operator *(OrbitalVector vector, decimal scalar) => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        public static OrbitalVector operator /(OrbitalVector vector, decimal scalar) => new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    }
}
