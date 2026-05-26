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
```

Full endpoint documentation:

- [API Endpoints](./docs/api-endpoints.md)

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

## Security

Implemented backend security practices:

- BCrypt password hashing.
- JWT Bearer authentication.
- Role-based authorization.
- DTO-based input validation.
- EF Core parameterized queries to reduce SQL injection risk.
- Secrets excluded from Git through `.gitignore`.

## Next Steps

1. Build the Wokwi ESP32 simulation.
2. Integrate ESP32 with the backend endpoints.
3. Build the Android Kotlin app with MVVM and Jetpack Compose.
4. Collect final screenshots, logs and test evidence for submission.
