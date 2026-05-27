package com.orbitshield.engineer.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.orbitshield.engineer.data.local.SessionStore
import com.orbitshield.engineer.domain.model.AuthSession
import com.orbitshield.engineer.domain.usecase.LoginUseCase
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class AuthUiState(
    val baseUrl: String = "http://10.0.2.2:5184",
    val email: String = "orbit.demo.2026@orbitshield.local",
    val password: String = "OrbitShield123!",
    val loading: Boolean = false,
    val error: String? = null
)

class AuthViewModel(
    private val loginUseCase: LoginUseCase,
    private val sessionStore: SessionStore
) : ViewModel() {
    private val _uiState = MutableStateFlow(AuthUiState())
    val uiState: StateFlow<AuthUiState> = _uiState.asStateFlow()

    fun updateBaseUrl(value: String) {
        _uiState.value = _uiState.value.copy(baseUrl = value, error = null)
    }

    fun updateEmail(value: String) {
        _uiState.value = _uiState.value.copy(email = value, error = null)
    }

    fun updatePassword(value: String) {
        _uiState.value = _uiState.value.copy(password = value, error = null)
    }

    fun login() {
        val state = _uiState.value
        viewModelScope.launch {
            _uiState.value = state.copy(loading = true, error = null)
            runCatching {
                loginUseCase(state.baseUrl, state.email, state.password)
            }.onSuccess { session ->
                sessionStore.save(session)
                _uiState.value = state.copy(loading = false, error = null)
            }.onFailure { error ->
                _uiState.value = state.copy(
                    loading = false,
                    error = error.message ?: "Unable to authenticate with Mission Control."
                )
            }
        }
    }

    fun logout() {
        viewModelScope.launch { sessionStore.clear() }
    }

    companion object {
        fun factory(
            loginUseCase: LoginUseCase,
            sessionStore: SessionStore
        ): ViewModelProvider.Factory = object : ViewModelProvider.Factory {
            @Suppress("UNCHECKED_CAST")
            override fun <T : ViewModel> create(modelClass: Class<T>): T =
                AuthViewModel(loginUseCase, sessionStore) as T
        }
    }
}

data class SessionUiState(
    val session: AuthSession? = null
)
