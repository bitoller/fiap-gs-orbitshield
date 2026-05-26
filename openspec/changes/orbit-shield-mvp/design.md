# Design Notes

## System Shape

```text
Wokwi ESP32 Satellite
        |
        | HTTP via public tunnel
        v
.NET 8 Mission Control API
        |
        | EF Core Oracle provider
        v
Oracle Database

Android Engineer Panel
        |
        | REST + JWT
        v
.NET 8 Mission Control API
```

## Backend Decisions

- Use Clean Architecture style projects:
  - `OrbitShield.Domain`
  - `OrbitShield.Application`
  - `OrbitShield.Infrastructure`
  - `OrbitShield.Api`
- Use Oracle tables prefixed with `OS_`.
- Use the connected Oracle user schema for FIAP compatibility.
- Use BCrypt for password hashing.
- Use JWT Bearer authentication and role-based authorization.
- Use public demo endpoints for the ESP32 because Wokwi cannot easily handle JWT token lifecycle.

## IoT Decisions

- Use ESP32 instead of Arduino Uno because Wi-Fi is required.
- Use Wokwi-GUEST for internet access.
- Use localtunnel for the Wokwi integration because ngrok redirected HTTP to HTTPS and HTTPS failed in the ESP32 simulation.
- Represent thrust with a potentiometer.
- Represent thermal telemetry with DHT22.
- Represent gravity as a simulated telemetry value.
- Represent avoidance actuation with a servo motor at 90 degrees.
- Use REST polling every 5 seconds for the MVP automatic reaction loop.

## Space Domain Interpretation

The Wokwi circuit is not a physical spacecraft implementation. It demonstrates the software integration pattern:

- Backend performs mission analysis.
- Satellite simulator sends telemetry.
- Satellite simulator reacts to a collision alert.
- Backend stores the maneuver and sensor readings.

In a real satellite, the servo would be replaced by reaction wheels, magnetorquers or thrusters controlled by flight software.

The prototype should be described as a software integration and decision-loop MVP. It validates telemetry ingestion, mission-control analysis, alert propagation and maneuver logging. It does not claim to simulate full spacecraft dynamics inside Wokwi.

## Lag Strategy

The MVP solves the automatic reaction requirement through periodic polling:

```text
ESP32 -> GET conjunction status every 5 seconds -> automatic actuator response
```

This bounds the delay to roughly one polling interval plus HTTP latency.

The best future architecture is MQTT/event-driven command delivery:

```text
Mission Control -> publish avoidance command -> Satellite subscribed topic -> immediate response
```

For real spacecraft-like behavior, the final architecture should also include local autonomy so the satellite can execute safety rules even during communication delay or outage.

## Android Follow-Up

The Android app should keep the previously agreed direction:

- Kotlin
- Jetpack Compose
- Retrofit with Gson
- MVVM
- StateFlow
- Polling every 3 to 5 seconds
- Login screen
- Dashboard screen
- Maneuver history screen
- Critical collision alert visual state
