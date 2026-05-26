# Backend Test Plan

| ID | Scenario | Input | Expected Output | Status |
| --- | --- | --- | --- | --- |
| T01 | Read default satellite telemetry | `GET /api/satellite/telemetry` | Returns `ORB-01`, fuel `82`, solar energy `94` | Passed |
| T02 | Simulate critical conjunction | `GET /api/satellite/conjunctions?simulateEmergency=true` | Returns `collisionRisk=true`, `distance=12`, `probability=98.5`, `debrisId=DEBRIS-2026` | Passed |
| T03 | Persist Wokwi sensor reading | `POST /api/satellite/sensor-readings` | Creates a sensor reading with temperature, radiation, voltage, gravity and thrust | Passed |
| T04 | Persist ESP32 maneuver | `POST /api/satellite/maneuver` | Returns `accepted=true` and stores the maneuver log | Passed |
| T05 | Authenticate engineer user | `POST /api/auth/register` and `POST /api/auth/login` | Password is hashed and login returns JWT Bearer token | Passed |
| T06 | Access protected maneuver history | `GET /api/satellite/maneuvers?satelliteId=1` with Bearer token | Returns maneuver history from Oracle | Passed |
| T07 | Store and retrieve TLE data | `POST /api/satellite/orbital-elements` and `GET /api/satellite/orbital-elements?satelliteId=1` | Stores and returns TLE lines from Oracle | Passed |

## Executed Evidence

FIAP Oracle schema validation:

```text
OS_CONJUNCTION_EVENTS
OS_DEBRIS_OBJECTS
OS_MANEUVER_LOGS
OS_ORBITAL_ELEMENTS
OS_SATELLITES
OS_SENSOR_READINGS
OS_USERS
```

Telemetry response:

```json
{
  "satelliteId": 1,
  "satelliteCode": "ORB-01",
  "fuel": 82,
  "solarEnergy": 94,
  "orbitStatus": "Nominal"
}
```

Emergency conjunction response:

```json
{
  "collisionRisk": true,
  "distance": 12,
  "probability": 98.5,
  "debrisId": "DEBRIS-2026",
  "status": "Critical"
}
```

JWT protected endpoint validation:

```json
{
  "registered": true,
  "tokenType": "Bearer",
  "maneuverCount": 1,
  "latestAction": "Collision avoidance maneuver executed"
}
```

TLE validation:

```json
{
  "createdTle": "ISS (ZARYA)",
  "tleCount": 1,
  "source": "CelesTrak"
}
```
