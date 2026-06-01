# Status Checkpoint

Last updated: 2026-05-25

## Completed

- Backend Mission Control API is implemented and builds with zero warnings.
- Oracle schema exists in the FIAP user schema with 7 `OS_*` tables.
- Docker Compose runs Oracle and the .NET API.
- Wokwi ESP32 simulation connects to Wi-Fi and sends data to the backend through localtunnel.
- ESP32 sensor reading POST returns `201`.
- Backend provides orbital environment generated from CelesTrak TLE + SGP4 plus injected debris vectors.
- ESP32 calculates closest approach onboard.
- ESP32 maneuver POST returns `200`.

## Validated Wokwi Output

```text
Wi-Fi connected. IP: 10.10.0.2
POST status: 201
GET status: 200
Collision alert received.
Debris: DEBRIS-2026
POST status: 200
```

## Current Tunnel

The current Wokwi/physical-phone tunnel URL is printed by:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

The URL may change if localtunnel is restarted.

## Next Work

Android implementation is complete. Remaining delivery work is final evidence collection and video pitch.
