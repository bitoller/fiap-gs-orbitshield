package com.orbitshield.engineer.domain.repository

import com.orbitshield.engineer.domain.model.AuthSession
import com.orbitshield.engineer.domain.model.ManeuverLog
import com.orbitshield.engineer.domain.model.MissionSnapshot
import com.orbitshield.engineer.domain.model.OrbitalEnvironment

interface AuthRepository {
    suspend fun login(baseUrl: String, email: String, password: String): AuthSession
}

interface MissionRepository {
    suspend fun snapshot(token: String): MissionSnapshot
    suspend fun maneuverHistory(token: String): List<ManeuverLog>
}

interface ScenarioRepository {
    suspend fun throwRandomDebris(): OrbitalEnvironment
    suspend fun triggerPreset(preset: String): OrbitalEnvironment
}
