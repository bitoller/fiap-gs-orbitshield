package com.orbitshield.engineer

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.orbitshield.engineer.data.local.SessionStore
import com.orbitshield.engineer.data.remote.RetrofitProvider
import com.orbitshield.engineer.data.repository.AuthRepositoryImpl
import com.orbitshield.engineer.data.repository.MissionRepositoryImpl
import com.orbitshield.engineer.data.repository.ScenarioRepositoryImpl
import com.orbitshield.engineer.domain.usecase.GetMissionSnapshotUseCase
import com.orbitshield.engineer.domain.usecase.LoginUseCase
import com.orbitshield.engineer.domain.usecase.ThrowRandomDebrisUseCase
import com.orbitshield.engineer.domain.usecase.TriggerScenarioPresetUseCase
import com.orbitshield.engineer.presentation.AuthViewModel
import com.orbitshield.engineer.presentation.MissionViewModel
import com.orbitshield.engineer.ui.screens.AppTab
import com.orbitshield.engineer.ui.screens.DashboardScreen
import com.orbitshield.engineer.ui.screens.LoginScreen
import com.orbitshield.engineer.ui.screens.ManeuverHistoryScreen
import com.orbitshield.engineer.ui.screens.MissionScaffold
import com.orbitshield.engineer.ui.screens.ScenarioControlScreen
import com.orbitshield.engineer.ui.screens.SettingsScreen
import com.orbitshield.engineer.ui.theme.OrbitShieldTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        val sessionStore = SessionStore(applicationContext)
        setContent {
            OrbitShieldTheme {
                OrbitShieldApp(sessionStore = sessionStore)
            }
        }
    }
}

@Composable
private fun OrbitShieldApp(sessionStore: SessionStore) {
    val authViewModel: AuthViewModel = viewModel(
        factory = AuthViewModel.factory(
            loginUseCase = LoginUseCase(AuthRepositoryImpl()),
            sessionStore = sessionStore
        )
    )
    val authState by authViewModel.uiState.collectAsStateWithLifecycle()
    val session by sessionStore.session.collectAsStateWithLifecycle(initialValue = null)

    if (session == null) {
        LoginScreen(
            state = authState,
            onBaseUrlChange = authViewModel::updateBaseUrl,
            onEmailChange = authViewModel::updateEmail,
            onPasswordChange = authViewModel::updatePassword,
            onLogin = authViewModel::login
        )
        return
    }

    val activeSession = session ?: return
    val api = remember(activeSession.baseUrl) { RetrofitProvider.create(activeSession.baseUrl) }
    val missionRepository = remember(api) { MissionRepositoryImpl(api) }
    val scenarioRepository = remember(api) { ScenarioRepositoryImpl(api) }
    val missionViewModel: MissionViewModel = viewModel(
        key = activeSession.accessToken,
        factory = MissionViewModel.factory(
            token = activeSession.accessToken,
            getMissionSnapshotUseCase = GetMissionSnapshotUseCase(missionRepository),
            throwRandomDebrisUseCase = ThrowRandomDebrisUseCase(scenarioRepository),
            triggerScenarioPresetUseCase = TriggerScenarioPresetUseCase(scenarioRepository)
        )
    )
    val missionState by missionViewModel.uiState.collectAsStateWithLifecycle()
    var currentTab by rememberSaveable { mutableStateOf(AppTab.Dashboard) }

    MissionScaffold(
        currentTab = currentTab,
        onTabChange = { currentTab = it }
    ) { padding ->
        Box(modifier = Modifier.padding(padding)) {
            when (currentTab) {
                AppTab.Dashboard -> DashboardScreen(missionState)
                AppTab.Logs -> ManeuverHistoryScreen(missionState)
                AppTab.Global -> ScenarioControlScreen(
                    state = missionState,
                    onThrowRandom = missionViewModel::throwRandomDebris,
                    onPreset = missionViewModel::triggerPreset
                )
                AppTab.Settings -> SettingsScreen(
                    state = missionState,
                    userName = activeSession.userName,
                    userRole = activeSession.userRole,
                    baseUrl = activeSession.baseUrl,
                    onLogout = authViewModel::logout
                )
            }
        }
    }
}
