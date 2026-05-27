package com.orbitshield.engineer.data.repository

import com.orbitshield.engineer.data.mapper.toDomain
import com.orbitshield.engineer.data.remote.OrbitShieldApiService
import com.orbitshield.engineer.domain.model.MissionSnapshot
import com.orbitshield.engineer.domain.repository.MissionRepository

class MissionRepositoryImpl(
    private val api: OrbitShieldApiService
) : MissionRepository {
    override suspend fun snapshot(token: String): MissionSnapshot {
        val telemetry = runCatching { api.getTelemetry().toDomain() }.getOrNull()
        val sensor = runCatching { api.getLatestSensorReading().toDomain() }.getOrNull()
        val environment = runCatching { api.getOrbitalEnvironment().toDomain() }.getOrNull()
        val maneuvers = runCatching { maneuverHistory(token) }.getOrDefault(emptyList())

        return MissionSnapshot(
            telemetry = telemetry,
            sensorReading = sensor,
            orbitalEnvironment = environment,
            maneuvers = maneuvers
        )
    }

    override suspend fun maneuverHistory(token: String) =
        api.getManeuvers(authorization = "Bearer $token").map { it.toDomain() }
}
