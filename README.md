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
- Backend test plan and ER diagram documentation.

## Requirements

- Docker Desktop
- .NET SDK 8, only if running outside Docker

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

Build and start the API and Oracle:

```powershell
docker compose up --build
```

The API will be available at:

```text
http://localhost:5184
```

Swagger:

```text
http://localhost:5184/swagger
```

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
POST /api/orbital-scenarios/satellites/{satelliteId}/spawn-debris
GET  /api/orbital-scenarios/satellites/{satelliteId}/environment
```

Full endpoint documentation:

- [API Endpoints](./docs/api-endpoints.md)
- [IoT Test Evidence](./docs/iot-test-plan.md)
- [Orbital Autonomy Model](./docs/orbital-autonomy.md)

## IoT Simulation Scope

The Wokwi ESP32 will simulate the satellite hardware layer:

- Servo motor as collision avoidance actuator.
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

After a preset is triggered, the ESP32 reads the new environment on the next polling cycle and recalculates its maneuver autonomously. The actuator is no longer fixed at 90 degrees: the ESP32 computes a maneuver angle from miss distance, time to closest approach and simulated thrust, then returns to the nominal position when the next scenario is safe.

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

## Next Steps

1. Open the Wokwi ESP32 simulation in [iot/wokwi-orbit-shield](./iot/wokwi-orbit-shield).
2. Expose the backend with a public HTTP tunnel and update `Config.h`.
3. Build the Android Kotlin app with MVVM and Jetpack Compose.
4. Collect final screenshots, logs and test evidence for submission.
