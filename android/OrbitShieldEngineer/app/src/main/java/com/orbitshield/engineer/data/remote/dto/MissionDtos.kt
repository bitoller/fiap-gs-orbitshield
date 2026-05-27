package com.orbitshield.engineer.data.remote.dto

data class TelemetryDto(
    val satelliteId: Int,
    val satelliteCode: String,
    val fuel: Int,
    val solarEnergy: Int,
    val orbitStatus: String
)

data class SensorReadingDto(
    val id: Int,
    val satelliteId: Int,
    val temperatureCelsius: Double,
    val radiationLevel: Double,
    val batteryVoltage: Double,
    val simulatedGravity: Double?,
    val simulatedThrust: Double?,
    val createdAt: String
)

data class ManeuverLogDto(
    val id: Int,
    val satelliteId: Int,
    val conjunctionEventId: Int?,
    val action: String,
    val servoAngle: Int,
    val thrustLevel: Double?,
    val source: String,
    val executedAt: String
)

data class OrbitalVectorDto(
    val x: Double,
    val y: Double,
    val z: Double
)

data class OrbitalEnvironmentDto(
    val satelliteId: Int,
    val satelliteCode: String,
    val source: String,
    val tleName: String,
    val tleLine1: String,
    val tleLine2: String,
    val epochUtc: String,
    val satellitePositionKm: OrbitalVectorDto,
    val satelliteVelocityKmS: OrbitalVectorDto,
    val relativePositionKm: OrbitalVectorDto,
    val relativeVelocityKmS: OrbitalVectorDto,
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
    val probabilityModel: String,
    val recommendedEmergency: Boolean,
    val predictedImpact: Boolean
)
