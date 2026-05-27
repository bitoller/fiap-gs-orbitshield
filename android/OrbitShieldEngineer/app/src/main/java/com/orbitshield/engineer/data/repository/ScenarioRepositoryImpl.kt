package com.orbitshield.engineer.data.repository

import com.orbitshield.engineer.data.mapper.toDomain
import com.orbitshield.engineer.data.remote.OrbitShieldApiService
import com.orbitshield.engineer.domain.repository.ScenarioRepository

class ScenarioRepositoryImpl(
    private val api: OrbitShieldApiService
) : ScenarioRepository {
    override suspend fun throwRandomDebris() =
        api.throwRandomDebris().toDomain()

    override suspend fun triggerPreset(preset: String) =
        api.triggerPreset(preset).toDomain()
}
