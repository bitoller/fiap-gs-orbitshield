package com.orbitshield.engineer.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.orbitshield.engineer.domain.model.MissionSnapshot
import com.orbitshield.engineer.domain.model.OrbitalEnvironment
import com.orbitshield.engineer.domain.usecase.GetMissionSnapshotUseCase
import com.orbitshield.engineer.domain.usecase.ThrowRandomDebrisUseCase
import com.orbitshield.engineer.domain.usecase.TriggerScenarioPresetUseCase
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class MissionUiState(
    val snapshot: MissionSnapshot = MissionSnapshot(null, null, null, emptyList()),
    val loading: Boolean = true,
    val actionLoading: Boolean = false,
    val lastScenario: OrbitalEnvironment? = null,
    val error: String? = null
)

class MissionViewModel(
    private val token: String,
    private val getMissionSnapshotUseCase: GetMissionSnapshotUseCase,
    private val throwRandomDebrisUseCase: ThrowRandomDebrisUseCase,
    private val triggerScenarioPresetUseCase: TriggerScenarioPresetUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(MissionUiState())
    val uiState: StateFlow<MissionUiState> = _uiState.asStateFlow()
    private var pollingJob: Job? = null

    init {
        startPolling()
    }

    fun refresh() {
        viewModelScope.launch {
            runCatching { getMissionSnapshotUseCase(token) }
                .onSuccess { snapshot ->
                    _uiState.value = _uiState.value.copy(
                        snapshot = snapshot,
                        loading = false,
                        error = null
                    )
                }
                .onFailure { error ->
                    _uiState.value = _uiState.value.copy(
                        loading = false,
                        error = error.message ?: "Unable to refresh mission data."
                    )
                }
        }
    }

    fun throwRandomDebris() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(actionLoading = true, error = null)
            runCatching { throwRandomDebrisUseCase() }
                .onSuccess { environment ->
                    _uiState.value = _uiState.value.copy(
                        actionLoading = false,
                        lastScenario = environment,
                        error = null
                    )
                    refresh()
                }
                .onFailure { error ->
                    _uiState.value = _uiState.value.copy(
                        actionLoading = false,
                        error = error.message ?: "Unable to throw random debris."
                    )
                }
        }
    }

    fun triggerPreset(preset: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(actionLoading = true, error = null)
            runCatching { triggerScenarioPresetUseCase(preset) }
                .onSuccess { environment ->
                    _uiState.value = _uiState.value.copy(
                        actionLoading = false,
                        lastScenario = environment,
                        error = null
                    )
                    refresh()
                }
                .onFailure { error ->
                    _uiState.value = _uiState.value.copy(
                        actionLoading = false,
                        error = error.message ?: "Unable to trigger preset."
                    )
                }
        }
    }

    private fun startPolling() {
        pollingJob?.cancel()
        pollingJob = viewModelScope.launch {
            while (true) {
                refresh()
                delay(5_000)
            }
        }
    }

    override fun onCleared() {
        pollingJob?.cancel()
        super.onCleared()
    }

    companion object {
        fun factory(
            token: String,
            getMissionSnapshotUseCase: GetMissionSnapshotUseCase,
            throwRandomDebrisUseCase: ThrowRandomDebrisUseCase,
            triggerScenarioPresetUseCase: TriggerScenarioPresetUseCase
        ): ViewModelProvider.Factory = object : ViewModelProvider.Factory {
            @Suppress("UNCHECKED_CAST")
            override fun <T : ViewModel> create(modelClass: Class<T>): T =
                MissionViewModel(
                    token,
                    getMissionSnapshotUseCase,
                    throwRandomDebrisUseCase,
                    triggerScenarioPresetUseCase
                ) as T
        }
    }
}
