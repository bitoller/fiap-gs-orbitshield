# Orbit Shield Android Engineer Panel

## Goal

The Android app is a native Kotlin + Jetpack Compose engineer panel for Orbit Shield.

It does not use mocked data. Every screen reads from the real .NET Mission Control API.

## Project Location

```text
android/OrbitShieldEngineer
```

Open this folder in Android Studio.

## Architecture

```text
data/
  local/
    SessionStore
  remote/
    OrbitShieldApiService
    RetrofitProvider
    dto/
  repository/

domain/
  model/
  repository/
  usecase/

presentation/
  AuthViewModel
  MissionViewModel

ui/
  components/
  screens/
  theme/
```

The data flow follows MVVM with Clean Architecture boundaries:

```text
Retrofit DTO -> Mapper -> Domain Model -> UseCase -> ViewModel -> StateFlow -> Compose UI
```

## Libraries

- Kotlin
- Jetpack Compose
- Material 3
- Navigation-style bottom tabs
- Retrofit
- GsonConverterFactory
- OkHttp Logging Interceptor
- Coroutines
- StateFlow
- DataStore Preferences

## Backend Requirements

Start the backend, Oracle database and tunnel first:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

Swagger:

```text
http://localhost:5184/swagger
```

## Build From Terminal

The Android project includes a Gradle Wrapper, so no global Gradle installation is required.

From the repository root, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-android.ps1
```

If an emulator or physical phone is already connected:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-android.ps1 -Install
```

Manual build option:

```powershell
cd android\OrbitShieldEngineer
```

If your terminal does not already know the Android SDK path, set it for the current PowerShell session:

```powershell
$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_SDK_ROOT="$env:LOCALAPPDATA\Android\Sdk"
```

Build the debug APK:

```powershell
.\gradlew.bat :app:assembleDebug
```

Validated result:

```text
BUILD SUCCESSFUL
```

The APK is generated at:

```text
android/OrbitShieldEngineer/app/build/outputs/apk/debug/app-debug.apk
```

## Why Android Is Not Inside Docker

Docker already handles the backend, Oracle database and public tunnel. The Android app should run locally through Android Studio because it depends on:

- Android SDK;
- emulator graphics and hardware acceleration;
- connected device access through `adb`;
- interactive UI debugging.

The project still keeps the Android workflow as automated as possible with:

- Gradle Wrapper in the Android project;
- `scripts/start-demo.ps1` to start services and prepare `local.properties`;
- `scripts/build-android.ps1` to build the APK;
- `scripts/build-android.ps1 -Install` to install the APK when a device is connected.

## API Base URL

The login screen has an editable `API Base URL` field.

Use this for Android Emulator:

```text
http://10.0.2.2:5184
```

Use the Docker localtunnel URL for a physical phone or when testing through the public tunnel:

```powershell
docker logs orbitshield-tunnel --tail 50
```

If Docker prints:

```text
your url is: https://example.loca.lt
```

Type this in the Android app:

```text
http://example.loca.lt
```

The app allows cleartext HTTP for the MVP because Wokwi/localtunnel HTTP is more reliable in this project.

## Demo Login

Register this user in Swagger if needed:

```json
{
  "name": "Orbit Shield Demo",
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!",
  "role": "Engineer"
}
```

Then login in the Android app:

```text
email: orbit.demo.2026@orbitshield.local
password: OrbitShield123!
```

The app stores the JWT access token in DataStore and uses it for protected endpoints such as maneuver history.

## Screens

### Login

Real endpoint:

```http
POST /api/auth/login
```

### Dashboard

Real endpoints:

```http
GET /api/satellite/telemetry
GET /api/satellite/sensor-readings/latest?satelliteId=1
GET /api/orbital-scenarios/satellites/1/environment
GET /api/satellite/maneuvers?satelliteId=1
```

The dashboard polls every 5 seconds and shows:

- mission risk state;
- fuel;
- solar energy;
- orbit status;
- debris class;
- miss distance;
- collision probability;
- latest sensor data.

### Maneuver Logs

Real endpoint:

```http
GET /api/satellite/maneuvers?satelliteId=1
```

Shows ESP32 maneuver logs persisted by the backend.

### Global / Scenario Control

Real endpoints:

```http
POST /api/orbital-scenarios/satellites/1/throw-random-debris
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=NearMiss
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=LateDetection
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=DenseDebrisField
```

This screen lets the engineer trigger real backend scenarios. The Wokwi ESP32 reacts on its next polling cycle.

### Settings / System Status

This screen is read-only for the MVP. It derives its values from real session and mission data:

- logged-in user;
- role;
- active API URL;
- TLE source;
- current risk state;
- latest sensor feed status;
- maneuver log count.

## Design Reference

The visual direction is based on the Stitch export in:

```text
docs/stitch-reference/stitch_orbit_shield_autonomous_evasion
```

Design principles:

- dark mission-control background;
- cyan for nominal/active states;
- amber for attention;
- red for collision risk;
- compact technical cards;
- mono-spaced data labels;
- bottom navigation.

## Validation Checklist

1. Start backend with Docker.
2. Open Android Studio.
3. Open `android/OrbitShieldEngineer`.
4. Sync Gradle.
5. Or build from terminal with `.\gradlew.bat :app:assembleDebug`.
6. Run the app on Android Emulator.
7. Use `http://10.0.2.2:5184` as API URL.
8. Login with the demo engineer.
9. Open Dashboard and confirm real telemetry appears.
10. Open Global and tap `Throw Random Debris`.
11. Confirm Dashboard updates risk state on the next polling cycle.
12. Run Wokwi at the same time and confirm the ESP32 reacts.
13. Open Logs and confirm ESP32 maneuvers appear after avoidance events.
