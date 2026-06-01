# Orbit Shield Wokwi Satellite

This project simulates the Orbit Shield satellite IoT layer with an ESP32.

## Hardware Simulation

- ESP32 DevKit: onboard satellite computer.
- Servo motor on GPIO 18: avoidance maneuver actuator.
- Red LED on GPIO 19: critical collision alert indicator.
- LCD I2C 16x2 on GPIO 21/22: satellite status display.
- DHT22 on GPIO 15: thermal sensor.
- Potentiometer on GPIO 34: simulated thrust input.

## Data Flow

```text
ESP32
  -> POST /api/satellite/sensor-readings
  -> GET  /api/orbital-scenarios/satellites/1/environment
  -> calculate closest approach onboard
  -> classify the event as SAFE PASS, NEAR MISS, AVOIDANCE REQUIRED or IMPACT PREDICTED
  -> if missDistance <= safeDistance:
       calculate servo angle from severity, urgency and thrust
       turn on the red alert LED
       LCD collision warning
       POST /api/satellite/maneuver
  -> if missDistance > safeDistance:
       turn off the red alert LED
       return servo to nominal position
```

## Automatic Reaction and Lag

The satellite simulator reacts automatically. No manual action is needed after the simulation starts.

Current MVP behavior:

- Every 5 seconds, the ESP32 requests the latest orbital environment.
- The backend returns relative position and velocity vectors generated from CelesTrak TLE + SGP4 plus an injected debris path.
- The backend includes debris diameter, estimated mass, impact energy and debris class.
- The ESP32 calculates closest approach locally.
- If `missDistance <= safeDistance`, the ESP32 calculates an avoidance angle from miss distance, time to closest approach and simulated thrust.
- The LCD displays user-friendly mission states instead of raw telemetry.
- The red LED turns on during collision-risk maneuvers.
- The ESP32 posts the maneuver log to Mission Control.
- As the debris passes, the backend advances its relative position over simulated time.
- When the calculated environment becomes safe again, the servo returns to the nominal position automatically.
- The serial monitor always prints what the satellite perceived, including `SAFE PASS`, `NEAR MISS`, `AVOIDANCE REQUIRED` or `IMPACT PREDICTED`.
- Temperature, radiation, battery and thrust stay in Serial Monitor logs, not on the LCD.

LCD states:

```text
SCANNING ORBIT: no active threat.
TRACKING DEBRIS / WAITING PASS: object nearby but no maneuver required.
AUTO AVOIDANCE: maneuver is being executed.
IMPACT RISK: late/high-risk object, avoidance attempted too late.
SAFE PASS / OBJECT PASSED: object cleared the safety zone.
```

Swagger can trigger named scenarios while Wokwi is running:

```text
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=NearMiss
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=LateDetection
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=DenseDebrisField
```

The next ESP32 polling cycle reads the selected environment and recalculates the maneuver autonomously.

The scenario keeps moving after it is triggered. The backend advances the debris relative position using its relative velocity, so the ESP32 can later see that the object has passed and return to nominal without a manual reset.

Swagger can also throw a random debris object at the satellite:

```text
POST /api/orbital-scenarios/satellites/1/spawn-random-debris
```

Example body:

```json
{
  "satelliteId": 1,
  "safeDistanceKm": 5,
  "minimumDiameterMeters": 0.05,
  "maximumDiameterMeters": 8.0
}
```

The backend randomizes object size, estimated mass, cross-section, density, closest approach and reaction time. The ESP32 then decides whether the object is safe, a near miss, an avoidance case or an impact-risk case.

For the fastest demonstration, use the no-body endpoint:

```text
POST /api/orbital-scenarios/satellites/1/throw-random-debris
```

This behaves like a Swagger button: it throws a randomized object using default size and safety limits.

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

Follow these steps exactly for the browser simulation.

### 1. Start Docker

Open Docker Desktop and wait until it is running.

### 2. Start Orbit Shield services

Open PowerShell in the repository root:

```powershell
cd C:\Users\bi_to\Desktop\fiap-gs-orbitshield
```

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

This starts:

```text
orbitshield-oracle
orbitshield-api
orbitshield-tunnel
```

The script prints the current public tunnel URL and resets the mission to `SAFE PASS`.

### 3. Get the public tunnel URL

Open a second PowerShell terminal in the same repository folder and run:

```powershell
docker logs orbitshield-tunnel --tail 50
```

Look for:

```text
your url is: https://example.loca.lt
```

Use the same host with `http://` in Wokwi.

Example:

```text
Docker log: https://example.loca.lt
Wokwi Config.h: http://example.loca.lt
```

### 4. Update `Config.h`

In Wokwi, open `Config.h` and update this line:

```cpp
constexpr const char* ApiBaseUrl = "http://example.loca.lt";
```

If the tunnel container restarts, the URL may change. Update `Config.h` in the Wokwi project again.

### 5. Create or open the ESP32 project

```text
wokwi.com -> New Project -> ESP32 -> Arduino
```

Add or replace these files in the Wokwi project:

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

Important: `diagram.json` controls the circuit. If the LCD, servo or red LED do not appear, replace `diagram.json` again.

### 6. Run the simulation

Click Run in Wokwi.

At startup:

- LCD should turn on.
- Servo should move during the self-test.
- ESP32 should connect to `Wokwi-GUEST`.
- Serial Monitor should show `POST status: 201` and `GET status: 200`.
- LCD should show `SCANNING ORBIT` when no maneuver is needed.

Expected serial monitor output:

```text
Wi-Fi connected. IP: 10.10.0.2
POST status: 201
GET status: 200
Onboard miss distance km: 0.74
Risk classification: AVOIDANCE REQUIRED
Debris class: Large debris
Autonomous collision risk detected onboard.
Servo angle: 72
POST status: 200
```

### 7. Throw random debris from Swagger

Open Swagger:

```text
http://localhost:5184/swagger
```

Use:

```http
POST /api/orbital-scenarios/satellites/1/throw-random-debris
```

Click `Try it out`, then `Execute`. No body is needed.

On the next Wokwi cycle, the ESP32 reads the new orbital environment and decides what to do.

### 8. Test specific scenarios

Use these Swagger endpoints:

```text
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=NearMiss
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=LateDetection
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=DenseDebrisField
```

Expected visual behavior:

```text
SAFE PASS: LCD shows SAFE PASS / OBJECT PASSED, LED off, servo nominal.
NEAR MISS: LCD shows TRACKING DEBRIS / WAITING PASS, LED off, servo nominal.
AVOIDANCE REQUIRED: LCD shows AUTO AVOIDANCE, LED on, servo moves, maneuver is posted.
IMPACT PREDICTED: LCD shows IMPACT RISK, LED on, warning is logged, avoidance is attempted but marked late.
```

### 9. Return to normal

Normally, the satellite returns to normal automatically after the debris passes. Wait a few polling cycles and look for:

```text
Risk classification: SAFE PASS
No avoidance required.
```

If you want to force the classroom demo back to normal immediately, trigger:

```http
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass
```

The next Wokwi polling cycle should return the servo to nominal and turn off the red LED.

### 10. Troubleshooting

If Wokwi logs `connection refused`, timeout, or fails to read the API:

- Check the current tunnel URL with `docker logs orbitshield-tunnel --tail 50`.
- Confirm `Config.h` uses `http://`.
- Paste the current URL into Wokwi.
- Stop and run the Wokwi simulation again.

If the backend works locally but Wokwi does not:

- Test Swagger at `http://localhost:5184/swagger`.
- Test the public URL in the browser with `/api/satellite/telemetry`.
- Replace `Config.h` with the newest URL if needed.

If nothing moves or lights up:

- Replace `diagram.json` in Wokwi.
- Confirm the project has `SatelliteActuator.h`, `AutonomyEngine.h`, `Models.h` and `Config.h` updated.

Safe-pass scenario output:

```text
GET status: 200
Onboard miss distance km: 6.53
Risk classification: SAFE PASS
Debris class: Fragment
No avoidance required.
Miss distance km: 6.53
```

Late impact-risk output:

```text
GET status: 200
Risk classification: IMPACT PREDICTED
Debris class: Large debris
Impact predicted: avoidance attempted too late.
Autonomous collision risk detected onboard.
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
