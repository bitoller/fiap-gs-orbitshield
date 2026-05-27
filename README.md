# Orbit Shield

Orbit Shield is an academic MVP for autonomous orbital traffic management. The system simulates a mission control backend, a satellite IoT actuator layer and a native Android engineer panel.

## Architecture

```text
Android Engineer Panel
        |
        | REST + JWT
        v
.NET 8 Mission Control API
Controller -> Service -> Repository
        |
        | Oracle EF Core
        v
Oracle Database

ESP32 Wokwi Satellite
        |
        | GET conjunction alerts
        | POST sensor readings and maneuvers
        v
.NET 8 Mission Control API
```

## Backend Status

The backend is implemented with Clean Architecture:

```text
src/
  OrbitShield.Api/
  OrbitShield.Application/
  OrbitShield.Domain/
  OrbitShield.Infrastructure/
```

Implemented features:

- REST API with Swagger.
- Oracle persistence with `OS_*` tables.
- Controller, Service and Repository layers.
- JWT authentication.
- BCrypt password hashing.
- Role-based access control.
- Satellite CRUD.
- Telemetry endpoint for IoT and mobile.
- Collision conjunction simulation.
- ESP32 maneuver logging.
- Sensor readings with simulated gravity and thrust.
- TLE orbital element storage.
- CelesTrak TLE + SGP4 orbital environment endpoint.
- Swagger-triggered orbital scenario presets.
- Onboard closest-approach autonomy flow for Wokwi.
- Variable ESP32 avoidance angle with safe-pass return.
- Native Android engineer panel with Retrofit, StateFlow, DataStore and Jetpack Compose.
- Backend test plan and ER diagram documentation.

## Requirements

- Docker Desktop
- Android Studio, only for running the native Android app/emulator
- Wokwi account, only for the ESP32 browser simulation
- .NET SDK 8, only if running the backend outside Docker

Docker covers the backend, Oracle database and public HTTP tunnel. Android Studio is still required for the emulator because Android needs the local SDK, graphics acceleration and `adb`.

## Beginner Quick Start

Use this flow when running the complete demo from zero.

### 1. Start Docker Desktop

Open Docker Desktop and wait until it says the engine is running.

### 2. Open the project folder in PowerShell

Use PowerShell in the repository root:

```powershell
cd C:\Users\bi_to\Desktop\fiap-gs-orbitshield
```

### 3. Start the backend, database and tunnel with one script

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

This script starts Docker in the background and prepares the Android project with the local Android SDK path. Docker will start:

```text
orbitshield-oracle
orbitshield-api
orbitshield-tunnel
```

It also prints the URLs you need for Swagger, Android and Wokwi.

The script also prepares the demo:

```text
registers the demo engineer user if needed
resets the orbital scenario to SAFE PASS
```

If you prefer the raw Docker command, use:

```powershell
docker compose up -d --build
```

If you use the raw Docker command, you may still need to register the demo user manually in Swagger before logging into Android.

### 4. Open Swagger

Open this URL in the browser:

```text
http://localhost:5184/swagger
```

Swagger is the backend control panel. Use it to register/login users and throw debris objects.

### 5. Get the Wokwi public URL

The `start-demo.ps1` script prints the public URL automatically. If you need to check it again, run:

```powershell
docker logs orbitshield-tunnel --tail 50
```

Find the line:

```text
your url is: https://example.loca.lt
```

Copy the URL, but use `http://` in Wokwi. Example:

```cpp
constexpr const char* ApiBaseUrl = "http://example.loca.lt";
```

Important limitation: localtunnel can generate a new URL if the tunnel restarts. Docker starts the tunnel automatically, but Wokwi cannot automatically edit its browser project files. If the URL changes, paste the new `http://...loca.lt` value into `Config.h` in Wokwi.

### 6. Open the Wokwi simulation

In Wokwi, create/open an ESP32 Arduino project and copy the files from:

```text
iot/wokwi-orbit-shield
```

Required files:

```text
sketch.ino
diagram.json
Config.h
Models.h
OrbitShieldHttpClient.h
MissionControlApi.h
AutonomyEngine.h
SatelliteSensors.h
SatelliteActuator.h
libraries.txt
```

Update `Config.h` with the tunnel URL from step 5, then click Run.

Expected Wokwi startup:

```text
Wi-Fi connected. IP: 10.10.0.2
POST status: 201
GET status: 200
Risk classification: ...
```

You should also see the LCD turn on, the servo self-test, and the red LED turn on only during risk states.

The LCD is intentionally user-friendly:

```text
SCANNING ORBIT
TRACKING DEBRIS
AUTO AVOIDANCE
IMPACT RISK
SAFE PASS
```

Raw telemetry such as temperature, radiation, battery and thrust stays in the Serial Monitor logs.

### 7. Throw random debris from Swagger

In Swagger, use this endpoint as the easiest demo button:

```http
POST /api/orbital-scenarios/satellites/1/throw-random-debris
```

Click `Try it out`, then `Execute`. No JSON body is needed.

On the next Wokwi polling cycle, the ESP32 will classify the event:

```text
SAFE PASS
NEAR MISS
AVOIDANCE REQUIRED
IMPACT PREDICTED
```

If the event requires avoidance, the red LED turns on, the servo moves, and the backend receives a maneuver log.

### 8. Watch the satellite return by itself

The active debris scenario advances over simulated time. The backend updates the relative position on every `GET /environment`, and the ESP32 recalculates the closest approach every polling cycle.

For demonstration speed, orbital scenario time is accelerated. That means a debris object can approach, trigger avoidance and then pass the satellite within a short classroom demo.

Each `Throw Random Debris` click creates one debris event. The event's initial risk decision stays stable while that object is approaching, so the dashboard does not look like several different objects were launched. After the object clears the closest-approach window, the same event becomes `SAFE PASS` and the satellite returns to nominal.

Expected sequence:

```text
AVOIDANCE REQUIRED -> SAFE PASS
```

When the object has passed:

```text
LCD shows SAFE PASS
red LED turns off
servo returns to nominal
```

### 9. Optional manual reset

Use this Swagger endpoint:

```http
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
```

Use this only when you want to force the demo back to a safe state immediately.

### 10. Optional Swagger authentication

Some endpoints require login. The IoT demo endpoints are public, but authentication is available for validation.

If you started with `scripts\start-demo.ps1`, the demo user is prepared automatically. If not, register it manually:

Register this user if needed:

```json
{
  "name": "Orbit Shield Demo",
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!",
  "role": "Engineer"
}
```

Then login with:

```json
{
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!"
}
```

Copy `accessToken`, click `Authorize` in Swagger, paste only the token value, and confirm.

### 11. Common Problems

If Wokwi shows Wi-Fi connected but API calls fail:

- Check the tunnel URL with `docker logs orbitshield-tunnel --tail 50`.
- Make sure `Config.h` uses `http://`, not `https://`.
- Paste the current tunnel URL into the Wokwi `Config.h`.
- Restart the Wokwi simulation.

If Swagger opens but Wokwi does not update:

- Wokwi only reads the active backend environment every 5 seconds.
- Wait one polling cycle after clicking `Execute`.
- Check the Serial Monitor for `GET status: 200`.

If login says user already exists but login fails:

- Register a different email, or use the existing known password for that email.
- For the demo user above, use exactly `OrbitShield123!`.

### 12. Run the Android App

Open this folder in Android Studio:

```text
android/OrbitShieldEngineer
```

Start an Android Emulator and run the app from Android Studio.

If you do not have an emulator yet:

```text
Android Studio -> Device Manager -> Create Device -> Phone -> Pixel or Medium Phone -> Download image if needed -> Finish -> Play
```

Use this API URL on the login screen:

```text
http://10.0.2.2:5184
```

Demo credentials:

```text
email: orbit.demo.2026@orbitshield.local
password: OrbitShield123!
```

You can also build the APK from PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-android.ps1
```

If an emulator or phone is already connected, build and install with:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-android.ps1 -Install
```

Android itself is not run inside Docker. Docker is used for the backend, Oracle database and public tunnel. Android Studio/emulator should stay local because it needs the Android SDK, device acceleration and a graphical emulator.

### 13. Demo Flow for the Presentation

Use this order when presenting:

```text
1. Start Docker with scripts\start-demo.ps1.
2. Open Swagger at http://localhost:5184/swagger.
3. Open Android app in the emulator and login.
4. Confirm Dashboard starts in SAFE PASS.
5. Open Wokwi and run the ESP32 simulation.
6. Trigger Throw Random Debris from Swagger or Android Global screen.
7. Watch Android show the random debris data.
8. Watch Wokwi LCD/LED/servo react if avoidance is required.
9. Wait for the object to pass and return to SAFE PASS.
10. Open maneuver logs in Android or Swagger.
```

## Environment Variables

Create a local `.env` file in the repository root:

```text
ORACLE_URL=jdbc:oracle:thin:@host:1521:service
ORACLE_USER=your_user
ORACLE_PASSWORD=your_password
```

Do not commit `.env` files. Use [.env.example](./.env.example) as the safe template.

For local Docker Oracle, use `.env.local` based on [.env.local.example](./.env.local.example).

## Run With Docker

Build and start Oracle, the API and the public Wokwi tunnel:

```powershell
docker compose up --build
```

Docker installs and runs all backend dependencies during the build. The API will be available at:

```text
http://localhost:5184
```

Swagger:

```text
http://localhost:5184/swagger
```

Public tunnel for Wokwi:

```powershell
docker logs orbitshield-tunnel --tail 50
```

Look for:

```text
your url is: https://example.loca.lt
```

Use the same address with `http://` in `iot/wokwi-orbit-shield/Config.h`, because the Wokwi ESP32 simulation is more reliable over plain HTTP:

```cpp
constexpr const char* ApiBaseUrl = "http://example.loca.lt";
```

If the tunnel container restarts, localtunnel can generate a new URL. In that case, update `Config.h` in Wokwi and run the simulation again. A fully automatic update inside Wokwi is not possible because Wokwi stores the sketch files in the browser project, outside Docker.

## Android Engineer Panel

The native Android app is located at:

```text
android/OrbitShieldEngineer
```

It uses the real Mission Control API only. There is no mocked telemetry in the app.

Build from terminal:

```powershell
cd android\OrbitShieldEngineer
$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_SDK_ROOT="$env:LOCALAPPDATA\Android\Sdk"
.\gradlew.bat :app:assembleDebug
```

Use this API URL in the Android Emulator:

```text
http://10.0.2.2:5184
```

Use the current localtunnel URL on a physical device:

```text
http://example.loca.lt
```

Full Android instructions:

- [Android Engineer Panel Setup](./docs/android-setup.md)

## Run Locally Without Docker

Restore dependencies:

```powershell
dotnet restore OrbitShield.sln
```

Build:

```powershell
dotnet build OrbitShield.sln
```

Run the API:

```powershell
dotnet run --project src/OrbitShield.Api/OrbitShield.Api.csproj --urls http://localhost:5184
```

## Database

Main Oracle scripts:

- [Create tables for the current Oracle user](./database/oracle/01_create_tables_current_user.sql)
- [Create local Oracle schema](./database/oracle/01_create_schema.sql)
- [Sample queries](./database/oracle/02_sample_queries.sql)
- [Drop current-user tables](./database/oracle/00_drop_tables_current_user.sql)

Academic database documentation:

- [ER Diagram](./docs/database-er-diagram.md)
- [Backend Test Plan](./docs/backend-test-plan.md)

## Main Endpoints

Authentication:

```http
POST /api/auth/register
POST /api/auth/login
```

Satellite CRUD:

```http
GET    /api/satellites
GET    /api/satellites/{id}
POST   /api/satellites
PUT    /api/satellites/{id}
DELETE /api/satellites/{id}
```

Mission Control:

```http
GET  /api/satellite/telemetry
GET  /api/satellite/conjunctions?simulateEmergency=true
POST /api/satellite/maneuver
GET  /api/satellite/maneuvers?satelliteId=1
POST /api/satellite/sensor-readings
GET  /api/satellite/sensor-readings/latest?satelliteId=1
POST /api/satellite/orbital-elements
GET  /api/satellite/orbital-elements?satelliteId=1
GET  /api/orbital-scenarios/satellites/{satelliteId}/presets
POST /api/orbital-scenarios/satellites/{satelliteId}/trigger-preset?preset=CriticalImpact
POST /api/orbital-scenarios/satellites/{satelliteId}/throw-random-debris
POST /api/orbital-scenarios/satellites/{satelliteId}/spawn-random-debris
POST /api/orbital-scenarios/satellites/{satelliteId}/spawn-debris
GET  /api/orbital-scenarios/satellites/{satelliteId}/environment
```

Full endpoint documentation:

- [API Endpoints](./docs/api-endpoints.md)
- [IoT Test Evidence](./docs/iot-test-plan.md)
- [Orbital Autonomy Model](./docs/orbital-autonomy.md)
- [Android Engineer Panel Setup](./docs/android-setup.md)

## IoT Simulation Scope

The Wokwi ESP32 will simulate the satellite hardware layer:

- Servo motor as collision avoidance actuator.
- Red LED as visual collision alert indicator.
- LCD I2C as mission alert display.
- Simulated sensor values for temperature, radiation, battery voltage, gravity and thrust.

Gravity and thrust are represented as simulated telemetry fields and persisted by the backend:

```json
{
  "simulatedGravity": 8.6942,
  "simulatedThrust": 72.5
}
```

The current autonomy flow is no longer a fixed collision placeholder. The backend obtains public TLE data from CelesTrak, propagates the satellite state with SGP4 and injects a debris approach vector. The ESP32 receives relative position and velocity and calculates closest approach locally before deciding to move the servo.

Swagger can trigger named orbital scenarios for live demonstrations:

```text
SafePass
NearMiss
CriticalImpact
LateDetection
DenseDebrisField
```

Use this endpoint while the Wokwi simulation is running:

```http
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
```

Or generate a random debris object with physical characteristics:

```http
POST /api/orbital-scenarios/satellites/1/spawn-random-debris
```

Example body:

```json
{
  "satelliteId": 1,
  "safeDistanceKm": 5,
  "minimumDiameterMeters": 0.05,
  "maximumDiameterMeters": 8.0
}
```

For the fastest Swagger demo, use the no-body endpoint:

```http
POST /api/orbital-scenarios/satellites/1/throw-random-debris
```

This acts like a button: it randomly throws an object with randomized size, estimated mass, cross-section, density, closest approach and reaction time. The ESP32 receives the updated environment on the next polling cycle and decides what to do.

After a preset is triggered, the ESP32 reads the new environment on the next polling cycle and recalculates its maneuver autonomously. The actuator is no longer fixed at 90 degrees: the ESP32 computes a maneuver angle from miss distance, time to closest approach and simulated thrust, then returns to the nominal position when the next scenario is safe.

The ESP32 also classifies what it perceived and shows it in the serial monitor and LCD:

```text
SAFE PASS
NEAR MISS
AVOIDANCE REQUIRED
IMPACT PREDICTED
```

The red LED turns on during risk/maneuver states and turns off when the next environment is safe.

The LCD shows mission states instead of raw telemetry:

```text
SCANNING ORBIT: no active threat.
TRACKING DEBRIS: nearby object, waiting for it to pass.
AUTO AVOIDANCE: autonomous maneuver in progress.
IMPACT RISK: late/high-risk object.
SAFE PASS: object cleared the safety zone.
```

The environment is time-aware. After debris is thrown, its relative position continues moving using its relative velocity. The initial classification of that single event remains stable during approach, and once the object has passed the closest-approach window the backend reports `SAFE PASS`. The ESP32 does not need a manual `SafePass` to recover: the next polling cycle returns the satellite to nominal. `SafePass` remains available as a manual classroom reset.

The orbital environment includes debris attributes:

```text
debrisDiameterMeters
estimatedMassKg
impactEnergyJoules
debrisClass
scenarioClassification
predictedImpact
```

Variable maneuver model:

```text
servoAngle = 25 + distanceSeverity * 40 + urgency * 15 + thrustAuthority * 10
```

The result is constrained between 25 and 90 degrees.

## Real Satellite Interpretation

The Wokwi circuit is a functional software-integration prototype, not a literal spacecraft hardware design.

In the MVP:

- The ESP32 represents an onboard satellite computer.
- The servo motor represents a visible avoidance actuator.
- The potentiometer represents simulated thrust intensity.
- The DHT22 represents thermal telemetry.
- The backend represents Mission Control and provides orbital environment data.

In a real satellite, the same software flow would be adapted to space-grade hardware:

- The ESP32 would be replaced by a radiation-tolerant onboard computer or flight controller.
- The servo movement would be replaced by reaction wheels, magnetorquers or chemical/electric thrusters.
- Simulated gravity and thrust values would come from orbital dynamics, IMU data, propulsion telemetry and flight software.
- Collision risk uses propagated orbital elements in the backend and an onboard closest-approach decision in the ESP32 simulation.

So the prototype validates the distributed architecture and decision loop:

```text
Telemetry -> Mission analysis -> Orbital vectors -> Onboard risk decision -> Maneuver log
```

It does not claim to reproduce full orbital mechanics inside Wokwi.

## Automatic Reaction and Lag Strategy

The MVP already demonstrates automatic satellite reaction:

```text
Backend propagates TLE and injects debris approach vector
        |
        v
ESP32 polls Mission Control every 5 seconds for orbital vectors
        |
        v
ESP32 calculates closest approach onboard
        |
        v
Servo moves according to maneuver severity and LCD shows collision alert
        |
        v
ESP32 posts the maneuver log back to the backend
```

This solves the core academic problem: the satellite simulator reacts automatically to a collision alert without manual intervention.

The current lag-control strategy is polling:

- ESP32 calls the backend every 5 seconds.
- Worst-case detection delay is approximately the polling interval plus HTTP latency.
- This is simple, free and reliable enough for the MVP demonstration.

Best future option:

- Replace polling with MQTT or another event-driven command channel.
- Mission Control would publish a collision-avoidance command.
- The satellite would subscribe and react immediately.
- This would reduce latency and better match distributed IoT/spacecraft command patterns.

Recommended evolution:

```text
MVP: REST polling every 5 seconds
Next version: MQTT command topic for near-real-time alerts
Advanced version: hybrid autonomy, where the satellite can react locally if communication is delayed
```

## Security

Implemented backend security practices:

- BCrypt password hashing.
- JWT Bearer authentication.
- Role-based authorization.
- DTO-based input validation.
- EF Core parameterized queries to reduce SQL injection risk.
- Secrets excluded from Git through `.gitignore`.

## Demo Authentication

Swagger may ask for a JWT because protected admin/engineer endpoints exist. IoT demo endpoints are public, but authentication is still available for professor validation.

Register an engineer:

```json
{
  "name": "Orbit Shield Demo",
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!",
  "role": "Engineer"
}
```

Login:

```json
{
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!"
}
```

Copy `accessToken`, click `Authorize` in Swagger and paste only the token value.

## Next Steps

1. Open the Wokwi ESP32 simulation in [iot/wokwi-orbit-shield](./iot/wokwi-orbit-shield).
2. Expose the backend with a public HTTP tunnel and update `Config.h`.
3. Open the Android Kotlin app in [android/OrbitShieldEngineer](./android/OrbitShieldEngineer).
4. Collect final screenshots, logs and test evidence for submission.
