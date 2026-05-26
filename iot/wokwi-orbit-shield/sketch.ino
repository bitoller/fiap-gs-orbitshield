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

    AutonomyDecision decision = autonomyEngine.evaluate(environment);
    Serial.print("Onboard miss distance km: ");
    Serial.println(decision.missDistanceKm);

    if (decision.collisionRisk)
    {
        satelliteActuator.triggerAutonomousAvoidance(decision, reading.simulatedThrust);

        ManeuverCommand maneuver;
        maneuver.satelliteId = Config::SatelliteId;
        maneuver.servoAngle = Config::ServoAvoidanceAngle;
        maneuver.thrustLevel = reading.simulatedThrust;
        missionControl.postManeuver(maneuver);
    }
}
