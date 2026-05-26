# Orbital Autonomy Model

## Goal

The autonomy flow avoids making the backend issue a direct maneuver command. Mission Control provides the orbital environment, and the ESP32 satellite simulator decides whether it must maneuver.

```text
CelesTrak TLE -> Backend SGP4 propagation -> Injected debris vector
        -> ESP32 local closest-approach calculation
        -> Servo avoidance maneuver
        -> Maneuver log persisted in Oracle
```

## Free Orbital Data Source

The backend uses public CelesTrak TLE data for the ISS:

```text
https://celestrak.org/NORAD/elements/gp.php?CATNR=25544&FORMAT=TLE
```

The current implementation propagates the TLE using the free `SGP.NET` library.

## Debris Injection

For the MVP, the debris object is synthetic but physically meaningful:

- The satellite state comes from real TLE + SGP4 propagation.
- The debris is injected as a relative position and relative velocity vector.
- The vector is chosen to cross the satellite safety volume in the lookahead window.

This models the project premise:

```text
Something is placed on an intercept path.
The satellite receives the environment.
The satellite reacts autonomously.
```

## Onboard Closest-Approach Calculation

The ESP32 calculates:

```text
tca = -dot(relativePosition, relativeVelocity) / |relativeVelocity|^2
missDistance = |relativePosition + relativeVelocity * tca|
risk = missDistance <= safeDistance
```

If risk is true, the ESP32 moves the servo to 90 degrees and posts the maneuver to Mission Control.

The current Wokwi behavior uses a variable actuator angle instead of a fixed movement:

```text
servoAngle = 25 + distanceSeverity * 40 + urgency * 15 + thrustAuthority * 10
```

Where:

- `distanceSeverity` increases as miss distance gets closer to zero;
- `urgency` increases as time to closest approach gets shorter;
- `thrustAuthority` comes from the potentiometer-simulated propulsion value.

The result is constrained between 25 and 90 degrees. If a later scenario is safe, the ESP32 returns the servo to the nominal angle.

## Named Test Scenarios

The backend exposes presets for live Swagger demonstrations:

```text
GET  /api/orbital-scenarios/satellites/1/presets
POST /api/orbital-scenarios/satellites/1/trigger-preset?preset=CriticalImpact
```

Available presets:

- `SafePass`
- `NearMiss`
- `CriticalImpact`
- `LateDetection`
- `DenseDebrisField`

## Probability Formula From The Presentation

Page 7 of the project deck references:

```text
P_collision = 1 - e^(-integral(v * sigma * n dt))
```

In the MVP, the backend implements the discretized form:

```text
P_collision = 1 - exp(-(relativeSpeedKmS * effectiveCrossSectionKm2 * debrisDensityPerKm3 * lookaheadSeconds))
```

This probability is used as a risk indicator and explanation layer. The actual onboard decision remains autonomous and geometric:

```text
missDistance <= safeDistance
```

## Why This Is More Realistic Than A Placeholder

Earlier demo behavior returned a fixed `collisionRisk=true`.

The upgraded behavior:

- fetches real TLE data from CelesTrak;
- propagates satellite position and velocity using SGP4;
- injects a debris vector on a collision-like path;
- sends vectors, not commands, to the ESP32;
- lets the ESP32 calculate the risk locally;
- logs the maneuver only after autonomous action.

This preserves the academic MVP scope while matching the core premise: autonomy reduces reaction lag.
