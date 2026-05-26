# Orbit Shield Backend Setup

## Requirements

- .NET SDK 8
- Docker Desktop
- Oracle Database Free container

## Beginner Full Run

Use this flow when you only want to run and demonstrate the backend, without changing code.

### 1. Start Docker Desktop

Open Docker Desktop and wait until the engine is running.

### 2. Open PowerShell in the project folder

```powershell
cd C:\Users\bi_to\Desktop\fiap-gs-orbitshield
```

### 3. Start everything

```powershell
docker compose up --build
```

This command installs backend dependencies during the Docker build and starts:

```text
orbitshield-oracle
orbitshield-api
orbitshield-tunnel
```

Keep the terminal open while demonstrating.

### 4. Open Swagger

```text
http://localhost:5184/swagger
```

Swagger is the backend test screen. It lets you test authentication, telemetry, maneuvers and orbital scenarios.

### 5. Get the public tunnel URL for Wokwi

Open a second PowerShell terminal in the same folder:

```powershell
docker logs orbitshield-tunnel --tail 50
```

Look for:

```text
your url is: https://example.loca.lt
```

Use that URL with `http://` in Wokwi `Config.h`:

```cpp
constexpr const char* ApiBaseUrl = "http://example.loca.lt";
```

Limitation: localtunnel can change the URL after restart. Docker starts the tunnel automatically, but it cannot edit Wokwi browser files. If the URL changes, paste the new URL into Wokwi manually.

### 6. Authenticate in Swagger if needed

Many IoT demo endpoints are public, but protected admin/engineer endpoints require JWT.

Register:

```json
{
  "name": "Orbit Shield Demo",
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!",
  "role": "Engineer"
}
```

If Swagger says the user already exists, go straight to login.

Login:

```json
{
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!"
}
```

Copy `accessToken`, click `Authorize`, paste only the token value and confirm.

### 7. Throw a random object

In Swagger, use:

```http
POST /api/orbital-scenarios/satellites/1/throw-random-debris
```

No body is required. This is the easiest demonstration button.

The backend will randomly create a debris object with:

```text
diameter
estimated mass
cross-section
relative trajectory
closest approach
reaction time
impact prediction
```

The Wokwi ESP32 reads the new environment on its next 5-second polling cycle.

### 8. Automatic safe return

After a debris object is thrown, the active orbital scenario advances over simulated time. The backend updates the debris relative position on each environment request. The ESP32 polls the environment every few seconds and eventually sees that the object has passed.

Expected result:

```text
risk state -> updated relative position -> SAFE PASS
```

At that point Wokwi should return the servo to nominal and turn off the red LED.

### 9. Optional manual safe return

If you want to force the simulation back to normal immediately:

```http
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
```

The ESP32 should return the servo to nominal and turn off the red LED on the next polling cycle.

## Install Backend Dependencies

```powershell
dotnet restore OrbitShield.sln
```

When running through Docker, dependencies are restored automatically during image build:

```powershell
docker compose up --build
```

This command starts:

- `orbitshield-oracle`
- `orbitshield-api`
- `orbitshield-tunnel`

The API is exposed at:

```text
http://localhost:5184
```

The tunnel service exposes the API to Wokwi:

```powershell
docker logs orbitshield-tunnel --tail 50
```

Look for:

```text
your url is: https://example.loca.lt
```

Use the same address with `http://` in `iot/wokwi-orbit-shield/Config.h`.

The tunnel URL can change when the tunnel container restarts. Docker automates tunnel startup, but Wokwi still needs the current URL pasted into `Config.h` because Wokwi stores sketch files outside the Docker environment.

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

GET    /api/orbital-scenarios/satellites/1/presets
POST   /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
POST   /api/orbital-scenarios/satellites/1/throw-random-debris
POST   /api/orbital-scenarios/satellites/1/spawn-random-debris
GET    /api/orbital-scenarios/satellites/1/environment
```

## Demo Payloads

Register an engineer:

```json
{
  "name": "Orbit Shield Demo",
  "email": "orbit.demo.2026@orbitshield.local",
  "password": "OrbitShield123!",
  "role": "Engineer"
}
```

Login with the same email and password, copy `accessToken`, click `Authorize` in Swagger and paste only the token value.

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
