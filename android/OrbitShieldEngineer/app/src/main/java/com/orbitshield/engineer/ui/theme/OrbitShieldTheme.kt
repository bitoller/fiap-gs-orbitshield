package com.orbitshield.engineer.ui.theme

import androidx.compose.material3.ColorScheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

val DeepSpace = Color(0xFF0B0E14)
val Surface = Color(0xFF10131A)
val Panel = Color(0xFF191C22)
val PanelHigh = Color(0xFF272A31)
val CyberCyan = Color(0xFF00F0FF)
val CyanDim = Color(0xFF00AAB5)
val WarningAmber = Color(0xFFFFB800)
val EmergencyRed = Color(0xFFFF3B30)
val TextPrimary = Color(0xFFE1E2EB)
val TextMuted = Color(0xFF849495)

private val OrbitShieldColors: ColorScheme = darkColorScheme(
    primary = CyberCyan,
    onPrimary = DeepSpace,
    secondary = WarningAmber,
    error = EmergencyRed,
    background = Surface,
    surface = Panel,
    surfaceVariant = PanelHigh,
    onBackground = TextPrimary,
    onSurface = TextPrimary,
    onSurfaceVariant = TextMuted
)

@Composable
fun OrbitShieldTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = OrbitShieldColors,
        content = content
    )
}
