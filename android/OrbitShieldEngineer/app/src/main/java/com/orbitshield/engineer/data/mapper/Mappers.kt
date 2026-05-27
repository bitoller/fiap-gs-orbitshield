package com.orbitshield.engineer.data.mapper

import com.orbitshield.engineer.data.remote.dto.AuthResponseDto
import com.orbitshield.engineer.data.remote.dto.ManeuverLogDto
import com.orbitshield.engineer.data.remote.dto.OrbitalEnvironmentDto
import com.orbitshield.engineer.data.remote.dto.OrbitalVectorDto
import com.orbitshield.engineer.data.remote.dto.SensorReadingDto
import com.orbitshield.engineer.data.remote.dto.TelemetryDto
import com.orbitshield.engineer.domain.model.AuthSession
import com.orbitshield.engineer.domain.model.ManeuverLog
import com.orbitshield.engineer.domain.model.OrbitalEnvironment
import com.orbitshield.engineer.domain.model.OrbitalVector
import com.orbitshield.engineer.domain.model.SensorReading
import com.orbitshield.engineer.domain.model.Telemetry

fun AuthResponseDto.toDomain(baseUrl: String) = AuthSession(
    baseUrl = baseUrl,
    accessToken = accessToken,
    userName = user.name,
    userEmail = user.email,
    userRole = user.role
)

fun TelemetryDto.toDomain() = Telemetry(
    satelliteId = satelliteId,
    satelliteCode = satelliteCode,
    fuel = fuel,
    solarEnergy = solarEnergy,
    orbitStatus = orbitStatus
)

fun SensorReadingDto.toDomain() = SensorReading(
    temperatureCelsius = temperatureCelsius,
    radiationLevel = radiationLevel,
    batteryVoltage = batteryVoltage,
    simulatedGravity = simulatedGravity,
    simulatedThrust = simulatedThrust,
    createdAt = createdAt
)

fun OrbitalVectorDto.toDomain() = OrbitalVector(x = x, y = y, z = z)

fun OrbitalEnvironmentDto.toDomain() = OrbitalEnvironment(
    satelliteCode = satelliteCode,
    tleName = tleName,
    relativePositionKm = relativePositionKm.toDomain(),
    relativeVelocityKmS = relativeVelocityKmS.toDomain(),
    safeDistanceKm = safeDistanceKm,
    lookaheadSeconds = lookaheadSeconds,
    timeToClosestApproachSeconds = timeToClosestApproachSeconds,
    missDistanceKm = missDistanceKm,
    relativeSpeedKmS = relativeSpeedKmS,
    debrisDiameterMeters = debrisDiameterMeters,
    estimatedMassKg = estimatedMassKg,
    impactEnergyJoules = impactEnergyJoules,
    debrisClass = debrisClass,
    scenarioClassification = scenarioClassification,
    collisionProbability = collisionProbability,
    recommendedEmergency = recommendedEmergency,
    predictedImpact = predictedImpact
)

fun ManeuverLogDto.toDomain() = ManeuverLog(
    id = id,
    action = action,
    servoAngle = servoAngle,
    thrustLevel = thrustLevel,
    source = source,
    executedAt = executedAt
)
