# IoT Test Evidence

## Scenario

Validate the simulated ESP32 satellite in Wokwi communicating with the .NET Mission Control API through a public tunnel.

## Environment

- Backend: Docker Compose
- API URL exposed to Wokwi: `http://tender-socks-fall.loca.lt`
- Simulator: Wokwi ESP32 Arduino
- Circuit: ESP32, servo motor, LCD I2C, DHT22 and potentiometer

## Expected Flow

```text
ESP32 connects to Wokwi-GUEST
ESP32 posts sensor telemetry
ESP32 requests conjunction risk
Backend returns critical collision risk
ESP32 moves servo to 90 degrees
ESP32 displays collision alert on LCD
ESP32 posts maneuver log
Backend persists data in Oracle
```

## Executed Result

Serial monitor output confirmed:

```text
Wi-Fi connected. IP: 10.10.0.2
POST /api/satellite/sensor-readings -> 201
GET /api/satellite/conjunctions?simulateEmergency=true -> 200
Collision alert received.
Debris: DEBRIS-2026
POST /api/satellite/maneuver -> 200
```

Sample sensor payload:

```json
{
  "satelliteId": 1,
  "temperatureCelsius": 24,
  "radiationLevel": 18.56,
  "batteryVoltage": 4.1,
  "simulatedGravity": 8.6942,
  "simulatedThrust": 88
}
```

Sample maneuver payload:

```json
{
  "satelliteId": 1,
  "conjunctionEventId": null,
  "action": "Collision avoidance maneuver executed",
  "servoAngle": 90,
  "thrustLevel": 88,
  "source": "ESP32"
}
```

## Status

Passed.

The Wokwi simulation is integrated with the backend and Oracle persistence through the public tunnel.
