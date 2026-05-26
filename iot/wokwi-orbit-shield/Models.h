#pragma once

#include <Arduino.h>

struct ConjunctionAlert
{
    bool collisionRisk = false;
    float distance = 0.0f;
    float probability = 0.0f;
    String debrisId = "";
    String status = "Nominal";
};

struct SensorReading
{
    float temperatureCelsius = 0.0f;
    float radiationLevel = 0.0f;
    float batteryVoltage = 0.0f;
    float simulatedGravity = 0.0f;
    float simulatedThrust = 0.0f;
};

struct ManeuverCommand
{
    int satelliteId = 1;
    String action = "Collision avoidance maneuver executed";
    int servoAngle = 90;
    float thrustLevel = 0.0f;
    String source = "ESP32";
};
