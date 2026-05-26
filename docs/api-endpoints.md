# Orbit Shield API Endpoints

Base URL for local development:

```text
http://localhost:5184
```

## Authentication

| Method | Endpoint | Access | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | Public | Register users with BCrypt password hashing. |
| POST | `/api/auth/login` | Public | Authenticate and return a JWT Bearer token. |

## Satellite CRUD

| Method | Endpoint | Access | Purpose |
| --- | --- | --- | --- |
| GET | `/api/satellites` | Admin, Engineer | List satellites. |
| GET | `/api/satellites/{id}` | Admin, Engineer | Get one satellite. |
| POST | `/api/satellites` | Admin | Create a satellite. |
| PUT | `/api/satellites/{id}` | Admin | Update a satellite. |
| DELETE | `/api/satellites/{id}` | Admin | Delete a satellite. |

## Mission Control

| Method | Endpoint | Access | Purpose |
| --- | --- | --- | --- |
| GET | `/api/satellite/telemetry` | Public for IoT demo | Return fuel, solar energy and orbit status. |
| GET | `/api/satellite/conjunctions?simulateEmergency=true` | Public for IoT demo | Simulate or return collision risk. |
| POST | `/api/satellite/maneuver` | Public for IoT demo | Register ESP32 avoidance maneuver. |
| GET | `/api/satellite/maneuvers?satelliteId=1` | Admin, Engineer | Return recent maneuver logs. |
| POST | `/api/satellite/sensor-readings` | Public for IoT demo | Persist Wokwi sensor telemetry. |
| GET | `/api/satellite/sensor-readings/latest?satelliteId=1` | Public for IoT demo | Return latest sensor reading. |
| POST | `/api/satellite/orbital-elements` | Admin, Engineer | Store TLE orbital elements. |
| GET | `/api/satellite/orbital-elements?satelliteId=1` | Admin, Engineer | List TLE orbital elements. |
| GET | `/api/orbital-scenarios/satellites/{satelliteId}/presets` | Public for IoT demo | List named orbital scenarios for Swagger demonstrations. |
| POST | `/api/orbital-scenarios/satellites/{satelliteId}/trigger-preset?preset=CriticalImpact` | Public for IoT demo | Trigger a named orbital scenario for the ESP32 to recalculate autonomously. |
| POST | `/api/orbital-scenarios/satellites/{satelliteId}/spawn-debris` | Public for IoT demo | Inject a debris approach scenario around a propagated TLE orbit. |
| GET | `/api/orbital-scenarios/satellites/{satelliteId}/environment` | Public for IoT demo | Return relative orbital vectors for onboard autonomous risk calculation. |

## Orbital Scenario Presets

Swagger can trigger these named scenarios:

| Preset | Expected ESP32 behavior |
| --- | --- |
| `SafePass` | No maneuver. The actuator returns to nominal. |
| `NearMiss` | Mild avoidance maneuver near the safety boundary. |
| `CriticalImpact` | Strong avoidance maneuver for a deep safety-zone crossing. |
| `LateDetection` | Urgent maneuver because reaction time is short. |
| `DenseDebrisField` | Stronger probability signal from density and cross-section. |

Swagger is available at:

```text
http://localhost:5184/swagger
```
