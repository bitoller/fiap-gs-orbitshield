package com.orbitshield.engineer.data.remote

import com.orbitshield.engineer.data.remote.dto.AuthResponseDto
import com.orbitshield.engineer.data.remote.dto.LoginRequestDto
import com.orbitshield.engineer.data.remote.dto.ManeuverLogDto
import com.orbitshield.engineer.data.remote.dto.OrbitalEnvironmentDto
import com.orbitshield.engineer.data.remote.dto.SensorReadingDto
import com.orbitshield.engineer.data.remote.dto.TelemetryDto
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.POST
import retrofit2.http.Query

interface OrbitShieldApiService {
    @POST("api/auth/login")
    suspend fun login(@Body request: LoginRequestDto): AuthResponseDto

    @GET("api/satellite/telemetry")
    suspend fun getTelemetry(): TelemetryDto

    @GET("api/satellite/sensor-readings/latest")
    suspend fun getLatestSensorReading(@Query("satelliteId") satelliteId: Int = 1): SensorReadingDto

    @GET("api/orbital-scenarios/satellites/1/environment")
    suspend fun getOrbitalEnvironment(): OrbitalEnvironmentDto

    @GET("api/satellite/maneuvers")
    suspend fun getManeuvers(
        @Header("Authorization") authorization: String,
        @Query("satelliteId") satelliteId: Int = 1
    ): List<ManeuverLogDto>

    @POST("api/orbital-scenarios/satellites/1/throw-random-debris")
    suspend fun throwRandomDebris(): OrbitalEnvironmentDto

    @POST("api/orbital-scenarios/satellites/1/trigger-preset")
    suspend fun triggerPreset(@Query("preset") preset: String): OrbitalEnvironmentDto
}
