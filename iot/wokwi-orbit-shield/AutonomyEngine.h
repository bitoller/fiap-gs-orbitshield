#pragma once

#include <Arduino.h>
#include "Models.h"

class AutonomyEngine
{
public:
    AutonomyDecision evaluate(const OrbitalEnvironment& environment) const
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
};
