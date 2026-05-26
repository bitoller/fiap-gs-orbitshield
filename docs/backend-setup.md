# Orbit Shield Backend Setup

## Requirements

- .NET SDK 8
- Docker Desktop
- Oracle Database Free container

## Install Backend Dependencies

```powershell
dotnet restore OrbitShield.sln
```

When running through Docker, dependencies are restored automatically during image build:

```powershell
docker compose up --build
```

This command starts both:

- `orbitshield-oracle`
- `orbitshield-api`

The API is exposed at:

```text
http://localhost:5184
```

## Oracle FIAP Connection

The API loads Oracle credentials from the repository root `.env` file.

Expected variables:

```text
ORACLE_URL=jdbc:oracle:thin:@host:1521:service
ORACLE_USER=your_user
ORACLE_PASSWORD=your_password
```

The backend automatically converts the JDBC-style URL to the Oracle .NET provider format.

The `.env` file is ignored by Git. Use `.env.example` as the safe template.

Create tables in the connected FIAP schema:

```powershell
Get-Content database\oracle\01_create_tables_current_user.sql | docker exec -i orbitshield-oracle sqlplus -L <user>/<password>@oracle.fiap.com.br:1521/ORCL
```

For local Docker validation, create `.env.local`:

```text
ORACLE_URL=localhost:1521/FREEPDB1
ORACLE_USER=ORBITSHIELD
ORACLE_PASSWORD=OrbitShield123
```

Values in `.env.local` override `.env` for local development.

## Local Oracle Fallback

Use this option only when the FIAP Oracle database is unavailable.

## Start Oracle Database Free

```powershell
docker compose up -d oracle
```

Wait until the Oracle container reports that the database is ready.

## Create the Database Schema

Run `database/oracle/01_create_schema.sql` as a privileged Oracle user.

Expected application connection string:

```text
User Id=ORBITSHIELD;Password=OrbitShield123;Data Source=localhost:1521/FREEPDB1
```

## Run the API

```powershell
dotnet run --project src/OrbitShield.Api/OrbitShield.Api.csproj
```

Swagger is available in development mode at:

```text
https://localhost:7136/swagger
http://localhost:5184/swagger
```

## Main Endpoints

```http
POST   /api/auth/register
POST   /api/auth/login

GET    /api/satellites
GET    /api/satellites/{id}
POST   /api/satellites
PUT    /api/satellites/{id}
DELETE /api/satellites/{id}

GET    /api/satellite/telemetry
GET    /api/satellite/conjunctions?simulateEmergency=true
POST   /api/satellite/maneuver
POST   /api/satellite/sensor-readings
GET    /api/satellite/sensor-readings/latest?satelliteId=1
POST   /api/satellite/orbital-elements
GET    /api/satellite/orbital-elements?satelliteId=1
GET    /api/satellite/maneuvers?satelliteId=1
```

## Demo Payloads

Register an engineer:

```json
{
  "name": "Mission Engineer",
  "email": "engineer@orbitshield.local",
  "password": "Orbit@2026",
  "role": "Engineer"
}
```

Register a maneuver from ESP32:

```json
{
  "satelliteId": 1,
  "conjunctionEventId": null,
  "action": "Collision avoidance maneuver executed",
  "servoAngle": 90,
  "thrustLevel": 72.5,
  "source": "ESP32"
}
```

Register a sensor reading from Wokwi:

```json
{
  "satelliteId": 1,
  "temperatureCelsius": 28.4,
  "radiationLevel": 12.8,
  "batteryVoltage": 4.1,
  "simulatedGravity": 8.6942,
  "simulatedThrust": 72.5
}
```

Register TLE data:

```json
{
  "satelliteId": 1,
  "name": "ISS (ZARYA)",
  "tleLine1": "1 25544U 98067A   24164.51782528  .00016717  00000+0  10270-3 0  9008",
  "tleLine2": "2 25544  51.6393  73.2487 0006017  69.1765 291.0073 15.50034120  1234",
  "epoch": "2026-05-25T00:00:00Z",
  "source": "CelesTrak"
}
```
