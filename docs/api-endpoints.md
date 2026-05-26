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

Swagger is available at:

```text
http://localhost:5184/swagger
```
