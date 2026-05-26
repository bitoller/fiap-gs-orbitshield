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
  -> GET  /api/satellite/conjunctions?simulateEmergency=true
  -> if collisionRisk=true:
       servo to 90 degrees
       LCD collision warning
       POST /api/satellite/maneuver
```

## Automatic Reaction and Lag

The satellite simulator reacts automatically. No manual action is needed after the simulation starts.

Current MVP behavior:

- Every 5 seconds, the ESP32 requests the latest conjunction status.
- If the backend returns `collisionRisk=true`, the servo moves to 90 degrees.
- The LCD displays the collision alert.
- The ESP32 posts the maneuver log to Mission Control.

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
constexpr const char* ApiBaseUrl = "http://tender-socks-fall.loca.lt";
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
Collision alert received.
POST status: 200
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
