package com.orbitshield.engineer.domain.usecase

import com.orbitshield.engineer.domain.repository.AuthRepository
import com.orbitshield.engineer.domain.repository.MissionRepository
import com.orbitshield.engineer.domain.repository.ScenarioRepository

class LoginUseCase(private val repository: AuthRepository) {
    suspend operator fun invoke(baseUrl: String, email: String, password: String) =
        repository.login(baseUrl, email, password)
}

class GetMissionSnapshotUseCase(private val repository: MissionRepository) {
    suspend operator fun invoke(token: String) = repository.snapshot(token)
}

class GetManeuverHistoryUseCase(private val repository: MissionRepository) {
    suspend operator fun invoke(token: String) = repository.maneuverHistory(token)
}

class ThrowRandomDebrisUseCase(private val repository: ScenarioRepository) {
    suspend operator fun invoke() = repository.throwRandomDebris()
}

class TriggerScenarioPresetUseCase(private val repository: ScenarioRepository) {
    suspend operator fun invoke(preset: String) = repository.triggerPreset(preset)
}
