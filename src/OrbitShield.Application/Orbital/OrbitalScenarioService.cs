using OrbitShield.Application.Repositories;
using SGPdotNET.Observation;

namespace OrbitShield.Application.Orbital;

public sealed class OrbitalScenarioService(ISatelliteRepository satellites) : IOrbitalScenarioService
{
    private const int IssCatalogNumber = 25544;
    private const string CelesTrakTleUrl = "https://celestrak.org/NORAD/elements/gp.php?CATNR=25544&FORMAT=TLE";
    private const decimal SimulationTimeScale = 12m;
    private static readonly HttpClient HttpClient = new();
    private static ActiveOrbitalScenario? activeScenario;

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
        var debrisDiameterMeters = request.DiameterMeters.HasValue
            ? Clamp(request.DiameterMeters.Value, 0.01m, 20)
            : DiameterFromCrossSection(request.EffectiveCrossSectionM2);
        var estimatedMassKg = request.EstimatedMassKg.HasValue
            ? Clamp(request.EstimatedMassKg.Value, 0.001m, 20_000)
            : EstimateMassKg(debrisDiameterMeters);

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

        var impactEnergyJoules = 0.5m * estimatedMassKg * (analysis.RelativeSpeedKmS * 1000m) * (analysis.RelativeSpeedKmS * 1000m);
        var predictedImpact = analysis.MissDistanceKm <= 0.05m || (analysis.TimeToClosestApproachSeconds <= 30m && analysis.MissDistanceKm <= safeDistanceKm);
        var scenarioClassification = ClassifyScenario(analysis.TimeToClosestApproachSeconds, analysis.MissDistanceKm, safeDistanceKm, predictedImpact);

        activeScenario = new ActiveOrbitalScenario(
            satellite.Id,
            satellite.Code,
            tle,
            now,
            ToOrbitalVector(propagated.Position),
            ToOrbitalVector(propagated.Velocity),
            relativePosition,
            relativeVelocity,
            safeDistanceKm,
            approachSeconds * 2,
            debrisDiameterMeters,
            estimatedMassKg,
            request.DebrisDensityPerKm3,
            request.EffectiveCrossSectionM2);

        return BuildResponse(activeScenario, now);
    }

    private static OrbitalEnvironmentResponse BuildResponse(ActiveOrbitalScenario scenario, DateTime now)
    {
        var elapsedSeconds = Math.Max(0m, (decimal)(now - scenario.CreatedAtUtc).TotalSeconds) * SimulationTimeScale;
        var currentRelativePosition = scenario.InitialRelativePositionKm + (scenario.RelativeVelocityKmS * elapsedSeconds);
        var remainingLookaheadSeconds = Math.Max(30m, scenario.LookaheadSeconds - elapsedSeconds);

        var analysis = Analyze(
            currentRelativePosition,
            scenario.RelativeVelocityKmS,
            scenario.SafeDistanceKm,
            remainingLookaheadSeconds,
            scenario.DebrisDensityPerKm3,
            scenario.EffectiveCrossSectionM2);

        var impactEnergyJoules = 0.5m * scenario.EstimatedMassKg * (analysis.RelativeSpeedKmS * 1000m) * (analysis.RelativeSpeedKmS * 1000m);
        var predictedImpact = analysis.MissDistanceKm <= 0.05m || (analysis.TimeToClosestApproachSeconds <= 30m && analysis.MissDistanceKm <= scenario.SafeDistanceKm);
        var scenarioClassification = ClassifyScenario(analysis.TimeToClosestApproachSeconds, analysis.MissDistanceKm, scenario.SafeDistanceKm, predictedImpact);

        return new OrbitalEnvironmentResponse(
            scenario.SatelliteId,
            scenario.SatelliteCode,
            "CelesTrak TLE + SGP4 + injected debris vector",
            scenario.Tle.Name,
            scenario.Tle.Line1,
            scenario.Tle.Line2,
            now,
            ToResponse(scenario.SatellitePositionKm),
            ToResponse(scenario.SatelliteVelocityKmS),
            ToResponse(currentRelativePosition),
            ToResponse(scenario.RelativeVelocityKmS),
            scenario.SafeDistanceKm,
            remainingLookaheadSeconds,
            analysis.TimeToClosestApproachSeconds,
            analysis.MissDistanceKm,
            analysis.RelativeSpeedKmS,
            scenario.DebrisDiameterMeters,
            scenario.EstimatedMassKg,
            Math.Round(impactEnergyJoules, 2),
            ClassifyDebris(scenario.DebrisDiameterMeters),
            scenarioClassification,
            analysis.CollisionProbability,
            "P = 1 - exp(-(relativeSpeedKmS * effectiveCrossSectionKm2 * debrisDensityPerKm3 * lookaheadSeconds))",
            analysis.MissDistanceKm <= scenario.SafeDistanceKm || analysis.CollisionProbability >= 50,
            predictedImpact);
    }

    private static OrbitalVector ToOrbitalVector(SGPdotNET.Util.Vector3 vector) =>
        new((decimal)vector.X, (decimal)vector.Y, (decimal)vector.Z);

    public Task<OrbitalEnvironmentResponse?> SpawnRandomDebrisAsync(RandomDebrisRequest request, CancellationToken cancellationToken = default)
    {
        var diameter = Random.Shared.NextDecimal(
            Clamp(request.MinimumDiameterMeters, 0.01m, 20),
            Clamp(request.MaximumDiameterMeters, 0.01m, 20));

        var safeDistance = Clamp(request.SafeDistanceKm, 1, 25);
        var approachSeconds = Random.Shared.Next(20, 181);
        var missDistance = Random.Shared.NextDecimal(0, safeDistance * 2.2m);
        var crossSection = Math.Round((decimal)Math.PI * (diameter / 2m) * (diameter / 2m), 4);
        var density = Random.Shared.NextDecimal(0.000000003m, 0.000000060m);

        return SpawnDebrisAsync(new SpawnDebrisRequest(
            request.SatelliteId,
            approachSeconds,
            missDistance,
            safeDistance,
            density,
            crossSection,
            diameter,
            EstimateMassKg(diameter)), cancellationToken);
    }

    public Task<OrbitalEnvironmentResponse?> SpawnPresetAsync(int satelliteId, OrbitalScenarioPreset preset, CancellationToken cancellationToken = default) =>
        SpawnDebrisAsync(CreatePresetRequest(satelliteId, preset), cancellationToken);

    public async Task<OrbitalEnvironmentResponse?> GetEnvironmentAsync(int satelliteId, CancellationToken cancellationToken = default)
    {
        if (activeScenario is not null && activeScenario.SatelliteId == satelliteId)
        {
            return BuildResponse(activeScenario, DateTime.UtcNow);
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
                EffectiveCrossSectionM2: 0.01m,
                DiameterMeters: 0.1m,
                EstimatedMassKg: 0.2m),

            OrbitalScenarioPreset.NearMiss => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 120,
                MissDistanceKm: 4.8m,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000010m,
                EffectiveCrossSectionM2: 0.8m,
                DiameterMeters: 1.0m,
                EstimatedMassKg: 30m),

            OrbitalScenarioPreset.CriticalImpact => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 75,
                MissDistanceKm: 0.5m,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000015m,
                EffectiveCrossSectionM2: 3.14m,
                DiameterMeters: 2.0m,
                EstimatedMassKg: 240m),

            OrbitalScenarioPreset.LateDetection => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 25,
                MissDistanceKm: 1.2m,
                SafeDistanceKm: 5,
                DebrisDensityPerKm3: 0.000000020m,
                EffectiveCrossSectionM2: 3.14m,
                DiameterMeters: 2.0m,
                EstimatedMassKg: 240m),

            OrbitalScenarioPreset.DenseDebrisField => new SpawnDebrisRequest(
                satelliteId,
                ApproachTimeSeconds: 90,
                MissDistanceKm: 3.5m,
                SafeDistanceKm: 6,
                DebrisDensityPerKm3: 0.000000050m,
                EffectiveCrossSectionM2: 7.06m,
                DiameterMeters: 3.0m,
                EstimatedMassKg: 810m),

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

    private static decimal DiameterFromCrossSection(decimal crossSectionM2)
    {
        if (crossSectionM2 <= 0)
        {
            return 0.1m;
        }

        return Math.Round((decimal)Math.Sqrt((double)(4m * crossSectionM2 / (decimal)Math.PI)), 3);
    }

    private static decimal EstimateMassKg(decimal diameterMeters)
    {
        var radius = diameterMeters / 2m;
        var volumeM3 = (4m / 3m) * (decimal)Math.PI * radius * radius * radius;
        const decimal aluminumLikeDensityKgM3 = 2700m;
        return Math.Round(Math.Max(0.01m, volumeM3 * aluminumLikeDensityKgM3 * 0.25m), 2);
    }

    private static string ClassifyDebris(decimal diameterMeters) =>
        diameterMeters switch
        {
            < 0.1m => "Micro debris",
            < 1.0m => "Fragment",
            < 3.0m => "Large debris",
            _ => "Derelict object"
        };

    private static string ClassifyScenario(decimal timeToClosestApproachSeconds, decimal missDistanceKm, decimal safeDistanceKm, bool predictedImpact)
    {
        if (predictedImpact)
        {
            return "IMPACT PREDICTED";
        }

        if (missDistanceKm > safeDistanceKm)
        {
            return missDistanceKm <= safeDistanceKm * 1.5m ? "NEAR MISS" : "SAFE PASS";
        }

        return timeToClosestApproachSeconds <= 30m ? "LATE DETECTION" : "AVOIDANCE REQUIRED";
    }

    private sealed record CelesTrakTle(string Name, string Line1, string Line2);
    private sealed record OrbitalAnalysis(decimal TimeToClosestApproachSeconds, decimal MissDistanceKm, decimal RelativeSpeedKmS, decimal CollisionProbability);
    private sealed record ActiveOrbitalScenario(
        int SatelliteId,
        string SatelliteCode,
        CelesTrakTle Tle,
        DateTime CreatedAtUtc,
        OrbitalVector SatellitePositionKm,
        OrbitalVector SatelliteVelocityKmS,
        OrbitalVector InitialRelativePositionKm,
        OrbitalVector RelativeVelocityKmS,
        decimal SafeDistanceKm,
        decimal LookaheadSeconds,
        decimal DebrisDiameterMeters,
        decimal EstimatedMassKg,
        decimal DebrisDensityPerKm3,
        decimal EffectiveCrossSectionM2);

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

internal static class RandomExtensions
{
    public static decimal NextDecimal(this Random random, decimal min, decimal max)
    {
        if (max < min)
        {
            (min, max) = (max, min);
        }

        return min + ((decimal)random.NextDouble() * (max - min));
    }
}
