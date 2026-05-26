#include "AutonomyEngine.h"
#include "Config.h"
#include "MissionControlApi.h"
#include "OrbitShieldHttpClient.h"
#include "SatelliteActuator.h"
#include "SatelliteSensors.h"

OrbitShieldHttpClient networkClient;
MissionControlApi missionControl(networkClient);
SatelliteSensors satelliteSensors;
SatelliteActuator satelliteActuator;
AutonomyEngine autonomyEngine;

unsigned long lastPollingTime = 0;
bool avoidanceActive = false;
int lastServoAngle = Config::ServoNominalAngle;

void setup()
{
    Serial.begin(115200);
    satelliteSensors.begin();
    satelliteActuator.begin();
    networkClient.connectWifi();
}

void loop()
{
    if (millis() - lastPollingTime < Config::PollingIntervalMs)
    {
        return;
    }

    lastPollingTime = millis();

    if (!networkClient.isConnected())
    {
        networkClient.connectWifi();
    }

    SensorReading reading = satelliteSensors.read();
    satelliteActuator.showTelemetry(reading);
    missionControl.postSensorReading(reading);

    OrbitalEnvironment environment;
    if (!missionControl.getOrbitalEnvironment(environment))
    {
        return;
    }

    AutonomyDecision decision = autonomyEngine.evaluate(environment, reading);
    Serial.print("Onboard miss distance km: ");
    Serial.println(decision.missDistanceKm);

    if (decision.collisionRisk)
    {
        const bool shouldLogManeuver = !avoidanceActive || abs(decision.servoAngle - lastServoAngle) >= 3;

        satelliteActuator.triggerAutonomousAvoidance(decision, reading.simulatedThrust);
        avoidanceActive = true;
        lastServoAngle = decision.servoAngle;

        if (shouldLogManeuver)
        {
            ManeuverCommand maneuver;
            maneuver.satelliteId = Config::SatelliteId;
            maneuver.servoAngle = decision.servoAngle;
            maneuver.thrustLevel = reading.simulatedThrust;
            missionControl.postManeuver(maneuver);
        }

        return;
    }

    if (avoidanceActive)
    {
        satelliteActuator.showSafePass(decision);
        avoidanceActive = false;
        lastServoAngle = Config::ServoNominalAngle;
    }
}
