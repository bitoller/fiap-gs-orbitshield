#pragma once

#include <Arduino.h>
#include "Config.h"
#include "Models.h"

class AutonomyEngine
{
public:
    AutonomyDecision evaluate(const OrbitalEnvironment& environment, const SensorReading& reading) const
    {
        AutonomyDecision decision;

        const OrbitalVector r = environment.relativePositionKm;
        const OrbitalVector v = environment.relativeVelocityKmS;
        const float velocityNormSquared = dot(v, v);

        float timeToClosestApproach = 0.0f;
        if (velocityNormSquared > 0.000001f)
        {
            timeToClosestApproach = -dot(r, v) / velocityNormSquared;
        }

        timeToClosestApproach = constrain(timeToClosestApproach, 0.0f, environment.lookaheadSeconds);

        const OrbitalVector closestPosition = add(r, multiply(v, timeToClosestApproach));
        const float missDistance = length(closestPosition);
        const float relativeSpeed = length(v);

        decision.timeToClosestApproachSeconds = timeToClosestApproach;
        decision.missDistanceKm = missDistance;
        decision.relativeSpeedKmS = relativeSpeed;
        decision.collisionRisk = missDistance <= environment.safeDistanceKm;
        decision.predictedImpact = environment.predictedImpact;
        decision.debrisClass = environment.debrisClass;
        decision.riskLabel = classifyRisk(decision, environment);
        decision.alertLedOn = decision.collisionRisk || decision.predictedImpact;
        decision.servoAngle = calculateServoAngle(decision, environment, reading);

        return decision;
    }

private:
    static float dot(const OrbitalVector& left, const OrbitalVector& right)
    {
        return (left.x * right.x) + (left.y * right.y) + (left.z * right.z);
    }

    static float length(const OrbitalVector& vector)
    {
        return sqrt(dot(vector, vector));
    }

    static OrbitalVector add(const OrbitalVector& left, const OrbitalVector& right)
    {
        return { left.x + right.x, left.y + right.y, left.z + right.z };
    }

    static OrbitalVector multiply(const OrbitalVector& vector, float scalar)
    {
        return { vector.x * scalar, vector.y * scalar, vector.z * scalar };
    }

    static int calculateServoAngle(
        const AutonomyDecision& decision,
        const OrbitalEnvironment& environment,
        const SensorReading& reading)
    {
        if (!decision.collisionRisk)
        {
            return Config::ServoNominalAngle;
        }

        const float distanceSeverity = constrain(
            1.0f - (decision.missDistanceKm / max(environment.safeDistanceKm, 0.1f)),
            0.0f,
            1.0f);

        const float urgency = constrain(
            1.0f - (decision.timeToClosestApproachSeconds / max(environment.lookaheadSeconds, 1.0f)),
            0.0f,
            1.0f);

        const float thrustAuthority = constrain(reading.simulatedThrust / 100.0f, 0.0f, 1.0f);
        const float angle = 25.0f + (distanceSeverity * 40.0f) + (urgency * 15.0f) + (thrustAuthority * 10.0f);

        return constrain(static_cast<int>(round(angle)), 25, Config::ServoAvoidanceAngle);
    }

    static String classifyRisk(const AutonomyDecision& decision, const OrbitalEnvironment& environment)
    {
        if (environment.predictedImpact)
        {
            return "IMPACT RISK";
        }

        if (environment.backendClassification.length() > 0)
        {
            return environment.backendClassification;
        }

        if (!decision.collisionRisk)
        {
            const float nearMissLimit = environment.safeDistanceKm * 1.5f;
            return decision.missDistanceKm <= nearMissLimit ? "NEAR MISS" : "SAFE PASS";
        }

        if (decision.timeToClosestApproachSeconds <= 30.0f)
        {
            return "LATE DETECT";
        }

        const float criticalLimit = environment.safeDistanceKm * 0.35f;
        return decision.missDistanceKm <= criticalLimit ? "CRITICAL" : "AVOIDANCE";
    }
};
