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

struct OrbitalVector
{
    float x = 0.0f;
    float y = 0.0f;
    float z = 0.0f;
};

struct OrbitalEnvironment
{
    int satelliteId = 1;
    String satelliteCode = "ORB-01";
    String source = "";
    String tleName = "";
    OrbitalVector relativePositionKm;
    OrbitalVector relativeVelocityKmS;
    float safeDistanceKm = 5.0f;
    float lookaheadSeconds = 120.0f;
    float backendCollisionProbability = 0.0f;
};

struct AutonomyDecision
{
    bool collisionRisk = false;
    float timeToClosestApproachSeconds = 0.0f;
    float missDistanceKm = 0.0f;
    float relativeSpeedKmS = 0.0f;
    int servoAngle = 0;
};
