# Status Checkpoint

Last updated: 2026-05-25

## Completed

- Backend Mission Control API is implemented and builds with zero warnings.
- Oracle schema exists in the FIAP user schema with 7 `OS_*` tables.
- Docker Compose runs Oracle and the .NET API.
- Wokwi ESP32 simulation connects to Wi-Fi and sends data to the backend through localtunnel.
- ESP32 sensor reading POST returns `201`.
- ESP32 conjunction GET returns `200`.
- ESP32 detects `DEBRIS-2026` collision risk.
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

```text
http://tender-socks-fall.loca.lt
```

This URL may change if localtunnel is restarted.

## Next Work

Do not start Android until the user asks. When resumed, implement the native Android app with:

- Kotlin
- Jetpack Compose
- Retrofit
- Gson converter
- MVVM
- StateFlow
- Polling every 3 to 5 seconds
- Login screen
- Dashboard screen
- Maneuver history screen
- Critical collision alert UI
