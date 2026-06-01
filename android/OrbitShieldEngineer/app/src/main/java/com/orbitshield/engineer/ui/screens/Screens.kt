package com.orbitshield.engineer.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Login
import androidx.compose.material.icons.filled.Bolt
import androidx.compose.material.icons.filled.Dashboard
import androidx.compose.material.icons.filled.Public
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import com.orbitshield.engineer.domain.model.ManeuverLog
import com.orbitshield.engineer.domain.model.MissionSnapshot
import com.orbitshield.engineer.domain.model.OrbitalEnvironment
import com.orbitshield.engineer.domain.model.RiskLevel
import com.orbitshield.engineer.domain.model.riskLevel
import com.orbitshield.engineer.presentation.AuthUiState
import com.orbitshield.engineer.presentation.MissionUiState
import com.orbitshield.engineer.ui.components.KeyValue
import com.orbitshield.engineer.ui.components.KeyValueBlock
import com.orbitshield.engineer.ui.components.OrbitProgress
import com.orbitshield.engineer.ui.components.RiskBanner
import com.orbitshield.engineer.ui.components.TechnicalPanel
import com.orbitshield.engineer.ui.components.TelemetryValue
import com.orbitshield.engineer.ui.theme.CyberCyan
import com.orbitshield.engineer.ui.theme.DeepSpace
import com.orbitshield.engineer.ui.theme.EmergencyRed
import com.orbitshield.engineer.ui.theme.TextMuted
import com.orbitshield.engineer.ui.theme.WarningAmber

@Composable
fun LoginScreen(
    state: AuthUiState,
    onBaseUrlChange: (String) -> Unit,
    onEmailChange: (String) -> Unit,
    onPasswordChange: (String) -> Unit,
    onLogin: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(DeepSpace)
            .padding(24.dp),
        verticalArrangement = Arrangement.Center
    ) {
        Text(
            text = "ORBIT SHIELD",
            color = CyberCyan,
            style = MaterialTheme.typography.headlineLarge,
            fontWeight = FontWeight.Bold
        )
        Text(
            text = "Engineer Panel",
            color = TextMuted,
            fontFamily = FontFamily.Monospace
        )
        Spacer(Modifier.height(28.dp))
        TechnicalPanel(title = "Mission Control Access") {
            OutlinedTextField(
                value = state.baseUrl,
                onValueChange = onBaseUrlChange,
                label = { Text("API Base URL") },
                singleLine = true,
                modifier = Modifier.fillMaxWidth()
            )
            OutlinedTextField(
                value = state.email,
                onValueChange = onEmailChange,
                label = { Text("Email") },
                singleLine = true,
                modifier = Modifier.fillMaxWidth()
            )
            OutlinedTextField(
                value = state.password,
                onValueChange = onPasswordChange,
                label = { Text("Password") },
                singleLine = true,
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth()
            )
            Button(
                onClick = onLogin,
                enabled = !state.loading,
                colors = ButtonDefaults.buttonColors(containerColor = CyberCyan, contentColor = DeepSpace),
                modifier = Modifier.fillMaxWidth()
            ) {
                if (state.loading) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(18.dp),
                        strokeWidth = 2.dp,
                        color = DeepSpace
                    )
                } else {
                    Icon(Icons.AutoMirrored.Filled.Login, contentDescription = null)
                    Spacer(Modifier.width(8.dp))
                    Text("Login")
                }
            }
            state.error?.let {
                Text(text = it, color = EmergencyRed, fontFamily = FontFamily.Monospace)
            }
        }
    }
}

@Composable
fun MissionScaffold(
    currentTab: AppTab,
    onTabChange: (AppTab) -> Unit,
    content: @Composable (PaddingValues) -> Unit
) {
    Scaffold(
        containerColor = DeepSpace,
        bottomBar = {
            NavigationBar(containerColor = Color(0xFF121720)) {
                AppTab.entries.forEach { tab ->
                    NavigationBarItem(
                        selected = currentTab == tab,
                        onClick = { onTabChange(tab) },
                        icon = { Icon(tab.icon, contentDescription = tab.label) },
                        label = { Text(tab.label) }
                    )
                }
            }
        },
        content = content
    )
}

enum class AppTab(val label: String, val icon: androidx.compose.ui.graphics.vector.ImageVector) {
    Dashboard("Dashboard", Icons.Default.Dashboard),
    Logs("Logs", Icons.Default.Bolt),
    Global("Global", Icons.Default.Public),
    Settings("Settings", Icons.Default.Settings)
}

@Composable
fun DashboardScreen(state: MissionUiState) {
    val snapshot = state.snapshot
    val environment = snapshot.orbitalEnvironment
    val risk = environment.riskLevel()
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(14.dp)
    ) {
        item { Header("ORBIT SHIELD", "Live Mission Dashboard") }
        item {
            RiskBanner(
                label = environment?.scenarioClassification ?: "SCANNING ORBIT",
                riskLevel = risk
            )
        }
        item { DashboardMetrics(snapshot) }
        item { EnvironmentPanel(environment) }
        item { SensorPanel(snapshot) }
        if (state.loading) item { Text("Synchronizing telemetry...", color = TextMuted) }
        state.error?.let { item { Text(it, color = EmergencyRed) } }
    }
}

@Composable
private fun DashboardMetrics(snapshot: MissionSnapshot) {
    TechnicalPanel(title = "Live Telemetry") {
        Row(horizontalArrangement = Arrangement.spacedBy(20.dp), modifier = Modifier.fillMaxWidth()) {
            TelemetryValue("Fuel", "${snapshot.telemetry?.fuel ?: 0}%", CyberCyan)
            TelemetryValue("Solar", "${snapshot.telemetry?.solarEnergy ?: 0}%", CyberCyan)
        }
        KeyValue("Orbit Status", snapshot.telemetry?.orbitStatus ?: "Unknown")
        KeyValue("Satellite", snapshot.telemetry?.satelliteCode ?: "ORB-01")
        OrbitProgress(((snapshot.telemetry?.fuel ?: 0) / 100f), CyberCyan)
    }
}

@Composable
private fun EnvironmentPanel(environment: OrbitalEnvironment?) {
    TechnicalPanel(
        title = "Orbital Environment",
        accent = when (environment.riskLevel()) {
            RiskLevel.Impact, RiskLevel.Avoidance -> EmergencyRed
            RiskLevel.Attention -> WarningAmber
            RiskLevel.Nominal -> CyberCyan
        }
    ) {
        KeyValue("Debris Class", environment?.debrisClass ?: "No object")
        KeyValue("Miss Distance", environment?.missDistanceKm?.format(" km") ?: "--")
        KeyValue("Closest Approach", environment?.timeToClosestApproachSeconds?.format(" s") ?: "--")
        KeyValue("Collision Probability", environment?.collisionProbability?.format("%") ?: "--")
        KeyValue("Object Diameter", environment?.debrisDiameterMeters?.format(" m") ?: "--")
        KeyValue("Estimated Mass", environment?.estimatedMassKg?.format(" kg") ?: "--")
    }
}

@Composable
private fun SensorPanel(snapshot: MissionSnapshot) {
    TechnicalPanel(title = "Onboard Sensors") {
        KeyValue("Temperature", snapshot.sensorReading?.temperatureCelsius?.format(" C") ?: "--")
        KeyValue("Radiation", snapshot.sensorReading?.radiationLevel?.format("") ?: "--")
        KeyValue("Battery", snapshot.sensorReading?.batteryVoltage?.format(" V") ?: "--")
        KeyValue("Simulated Thrust", snapshot.sensorReading?.simulatedThrust?.format("%") ?: "--")
    }
}

@Composable
fun ManeuverHistoryScreen(state: MissionUiState) {
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        item { Header("Maneuver Logs", "Autonomous ESP32 actions") }
        items(state.snapshot.maneuvers) { ManeuverCard(it) }
        if (state.snapshot.maneuvers.isEmpty()) {
            item { Text("No maneuvers registered yet.", color = TextMuted) }
        }
    }
}

@Composable
private fun ManeuverCard(maneuver: ManeuverLog) {
    TechnicalPanel(title = "Maneuver ${maneuver.id}") {
        KeyValueBlock("Action", maneuver.action)
        KeyValue("Servo Angle", "${maneuver.servoAngle} deg", CyberCyan)
        KeyValue("Thrust", maneuver.thrustLevel?.format("%") ?: "--")
        KeyValue("Source", maneuver.source)
        KeyValue("Executed At", maneuver.executedAt)
    }
}

@Composable
fun ScenarioControlScreen(
    state: MissionUiState,
    onThrowRandom: () -> Unit,
    onPreset: (String) -> Unit
) {
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(14.dp)
    ) {
        item { Header("Global Debris", "Scenario control") }
        item {
            TechnicalPanel(title = "Live Debris Injection") {
                Button(
                    onClick = onThrowRandom,
                    enabled = !state.actionLoading,
                    colors = ButtonDefaults.buttonColors(containerColor = CyberCyan, contentColor = DeepSpace),
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Text("Throw Random Debris")
                }
                listOf("SafePass", "NearMiss", "CriticalImpact", "LateDetection", "DenseDebrisField").forEach { preset ->
                    OutlinedButton(
                        onClick = { onPreset(preset) },
                        enabled = !state.actionLoading,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text(preset)
                    }
                }
            }
        }
        item { EnvironmentPanel(state.lastScenario ?: state.snapshot.orbitalEnvironment) }
    }
}

@Composable
fun SettingsScreen(state: MissionUiState, userName: String, userRole: String, baseUrl: String, onLogout: () -> Unit) {
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(14.dp)
    ) {
        item { Header("Autonomy Settings", "Read-only system status") }
        item {
            TechnicalPanel(title = "Engineer Session") {
                KeyValue("User", userName)
                KeyValue("Role", userRole)
                KeyValue("API", baseUrl)
                OutlinedButton(onClick = onLogout, modifier = Modifier.fillMaxWidth()) {
                    Text("Logout")
                }
            }
        }
        item {
            TechnicalPanel(title = "Autonomy Health") {
                val environment = state.snapshot.orbitalEnvironment
                KeyValue("Mission Source", environment?.tleName ?: "Waiting for TLE")
                KeyValue("Risk State", environment?.scenarioClassification ?: "SCANNING ORBIT")
                KeyValue("Sensor Feed", if (state.snapshot.sensorReading != null) "ONLINE" else "NO DATA")
                KeyValue("Maneuver Logs", state.snapshot.maneuvers.size.toString())
            }
        }
    }
}

@Composable
private fun Header(title: String, subtitle: String) {
    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
        Text(text = title, color = CyberCyan, style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Bold)
        Text(text = subtitle, color = TextMuted, fontFamily = FontFamily.Monospace)
    }
}

private fun Double.format(suffix: String): String = "%.2f%s".format(this, suffix)
