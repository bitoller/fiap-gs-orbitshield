package com.orbitshield.engineer.domain.model

data class AuthSession(
    val baseUrl: String,
    val accessToken: String,
    val userName: String,
    val userEmail: String,
    val userRole: String
)

data class Telemetry(
    val satelliteId: Int,
    val satelliteCode: String,
    val fuel: Int,
    val solarEnergy: Int,
    val orbitStatus: String
)

data class SensorReading(
    val temperatureCelsius: Double,
    val radiationLevel: Double,
    val batteryVoltage: Double,
    val simulatedGravity: Double?,
    val simulatedThrust: Double?,
    val createdAt: String
)

data class OrbitalVector(
    val x: Double,
    val y: Double,
    val z: Double
)

data class OrbitalEnvironment(
    val satelliteCode: String,
    val tleName: String,
    val relativePositionKm: OrbitalVector,
    val relativeVelocityKmS: OrbitalVector,
    val safeDistanceKm: Double,
    val lookaheadSeconds: Double,
    val timeToClosestApproachSeconds: Double,
    val missDistanceKm: Double,
    val relativeSpeedKmS: Double,
    val debrisDiameterMeters: Double,
    val estimatedMassKg: Double,
    val impactEnergyJoules: Double,
    val debrisClass: String,
    val scenarioClassification: String,
    val collisionProbability: Double,
    val recommendedEmergency: Boolean,
    val predictedImpact: Boolean
)

data class ManeuverLog(
    val id: Int,
    val action: String,
    val servoAngle: Int,
    val thrustLevel: Double?,
    val source: String,
    val executedAt: String
)

data class MissionSnapshot(
    val telemetry: Telemetry?,
    val sensorReading: SensorReading?,
    val orbitalEnvironment: OrbitalEnvironment?,
    val maneuvers: List<ManeuverLog>
)

enum class RiskLevel {
    Nominal,
    Attention,
    Avoidance,
    Impact
}

fun OrbitalEnvironment?.riskLevel(): RiskLevel {
    if (this == null) return RiskLevel.Nominal
    if (predictedImpact || scenarioClassification == "IMPACT PREDICTED") return RiskLevel.Impact
    if (recommendedEmergency || scenarioClassification == "AVOIDANCE REQUIRED") return RiskLevel.Avoidance
    if (scenarioClassification == "NEAR MISS") return RiskLevel.Attention
    return RiskLevel.Nominal
}
