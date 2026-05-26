# Orbit Shield Wokwi Satellite

This project simulates the Orbit Shield satellite IoT layer with an ESP32.

## Hardware Simulation

- ESP32 DevKit: onboard satellite computer.
- Servo motor on GPIO 18: avoidance maneuver actuator.
- LCD I2C 16x2 on GPIO 21/22: satellite status display.
- DHT22 on GPIO 15: thermal sensor.
- Potentiometer on GPIO 34: simulated thrust input.

## Data Flow

```text
ESP32
  -> POST /api/satellite/sensor-readings
  -> GET  /api/orbital-scenarios/satellites/1/environment
  -> calculate closest approach onboard
  -> if missDistance <= safeDistance:
       calculate servo angle from severity, urgency and thrust
       LCD collision warning
       POST /api/satellite/maneuver
  -> if missDistance > safeDistance:
       return servo to nominal position
```

## Automatic Reaction and Lag

The satellite simulator reacts automatically. No manual action is needed after the simulation starts.

Current MVP behavior:

- Every 5 seconds, the ESP32 requests the latest orbital environment.
- The backend returns relative position and velocity vectors generated from CelesTrak TLE + SGP4 plus an injected debris path.
- The ESP32 calculates closest approach locally.
- If `missDistance <= safeDistance`, the ESP32 calculates an avoidance angle from miss distance, time to closest approach and simulated thrust.
- The LCD displays the collision alert.
- The ESP32 posts the maneuver log to Mission Control.
- If a later scenario is safe, the servo returns to the nominal position.

Swagger can trigger named scenarios while Wokwi is running:

```text
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=NearMiss
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=LateDetection
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=DenseDebrisField
```

The next ESP32 polling cycle reads the selected environment and recalculates the maneuver autonomously.

The actuator angle is calculated onboard:

```text
servoAngle = 25 + distanceSeverity * 40 + urgency * 15 + thrustAuthority * 10
```

The result is constrained between 25 and 90 degrees. This keeps the demo simple while proving that the satellite simulator is not just executing a fixed movement.

This polling strategy keeps the implementation simple and free for the academic MVP. The expected delay is up to one polling cycle plus the HTTP request time.

Best future improvement:

- Replace REST polling with MQTT.
- Use a command topic such as `orbit-shield/satellites/ORB-01/commands`.
- The backend publishes an avoidance command.
- The ESP32 receives the command and reacts without waiting for the next poll.

## Setup

1. Start the backend:

```powershell
docker compose up --build
```

2. Expose the backend to Wokwi using a public tunnel.

Recommended option for the current Wokwi simulation:

```powershell
npx -y localtunnel --port 5184
```

3. Copy the public URL and update `Config.h`.

Current tunnel used during validation:

```cpp
constexpr const char* ApiBaseUrl = "http://cold-falcons-guess.loca.lt";
```

Use `http://` for the ESP32 simulation.

4. Open Wokwi in the browser.

```text
wokwi.com -> New Project -> ESP32 -> Arduino
```

5. Add or replace these files in the Wokwi project:

```text
sketch.ino
diagram.json
Config.h
Models.h
OrbitShieldHttpClient.h
MissionControlApi.h
AutonomyEngine.h
SatelliteSensors.h
SatelliteActuator.h
libraries.txt
```

6. Run the simulation.

Expected serial monitor output:

```text
Wi-Fi connected. IP: 10.10.0.2
POST status: 201
GET status: 200
Onboard miss distance km: 0.74
Autonomous collision risk detected onboard.
Servo angle: 72
POST status: 200
```

Safe-pass scenario output:

```text
GET status: 200
Onboard miss distance km: 6.53
No avoidance required.
Miss distance km: 6.53
```

If the tunnel is restarted, the URL may change. Update `Config.h` again with the new public URL.

## ngrok Notes

ngrok was tested first. Its HTTPS URL worked from the desktop, but the ESP32 simulation failed during the HTTPS connection. Switching to HTTP caused ngrok to return `307 Temporary Redirect` to HTTPS.

For that reason, localtunnel is the current zero-cost tunnel used by the Wokwi simulation.

## Physical ESP32 Option

The same code can run on a physical ESP32. Keep these changes in mind:

- Use your real Wi-Fi SSID and password in `Config.h`.
- Keep the API exposed through HTTPS or host it on a reachable network address.
- Wire the physical components to the same GPIO pins used by the Wokwi diagram.
- For production-like hardware, replace `client.setInsecure()` with a proper TLS certificate strategy.

## Notes

Gravity and propulsion are simulated telemetry values. Orbital mechanics and collision analysis remain in the backend, while the ESP32 acts as the satellite actuator and telemetry sender.

The servo movement is a demonstrator for an avoidance actuator. In a real satellite, this would not be a hobby servo. The equivalent spacecraft mechanisms would be reaction wheels, magnetorquers or thrusters commanded by onboard flight software after mission-control analysis.

The goal of this circuit is to validate the distributed software loop, not to reproduce physical orbital mechanics in Wokwi:

```text
Telemetry -> Backend collision analysis -> ESP32 alert response -> Maneuver persistence
```

For a real spacecraft, the ESP32 would be replaced by space-qualified avionics, and the backend simulation would evolve into an orbital analysis pipeline using propagated TLE data, ephemerides and validated maneuver-planning models.
