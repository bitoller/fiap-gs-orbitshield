#pragma once

#include <Arduino.h>
#include <ArduinoJson.h>
#include "Config.h"
#include "Models.h"
#include "OrbitShieldHttpClient.h"

class MissionControlApi
{
public:
    explicit MissionControlApi(OrbitShieldHttpClient& networkClient) : network(networkClient)
    {
    }

    bool getConjunctionAlert(ConjunctionAlert& alert)
    {
        String body;
        const String url = baseUrl() + "/api/satellite/conjunctions?simulateEmergency=true";

        if (!network.get(url, body))
        {
            Serial.println("Failed to read conjunction alert.");
            Serial.println(body);
            return false;
        }

        JsonDocument document;
        const DeserializationError error = deserializeJson(document, body);
        if (error)
        {
            Serial.print("Invalid conjunction JSON: ");
            Serial.println(error.c_str());
            return false;
        }

        alert.collisionRisk = document["collisionRisk"] | false;
        alert.distance = document["distance"] | 0.0f;
        alert.probability = document["probability"] | 0.0f;
        alert.debrisId = document["debrisId"] | "";
        alert.status = document["status"] | "Nominal";

        return true;
    }

    bool getOrbitalEnvironment(OrbitalEnvironment& environment)
    {
        String body;
        const String url = baseUrl() + "/api/orbital-scenarios/satellites/" + String(Config::SatelliteId) + "/environment";

        if (!network.get(url, body))
        {
            Serial.println("Failed to read orbital environment.");
            Serial.println(body);
            return false;
        }

        JsonDocument document;
        const DeserializationError error = deserializeJson(document, body);
        if (error)
        {
            Serial.print("Invalid orbital environment JSON: ");
            Serial.println(error.c_str());
            return false;
        }

        environment.satelliteId = document["satelliteId"] | Config::SatelliteId;
        environment.satelliteCode = document["satelliteCode"] | "ORB-01";
        environment.source = document["source"] | "";
        environment.tleName = document["tleName"] | "";
        environment.safeDistanceKm = document["safeDistanceKm"] | 5.0f;
        environment.lookaheadSeconds = document["lookaheadSeconds"] | 120.0f;
        environment.backendCollisionProbability = document["collisionProbability"] | 0.0f;

        environment.relativePositionKm = readVector(document["relativePositionKm"]);
        environment.relativeVelocityKmS = readVector(document["relativeVelocityKmS"]);

        return true;
    }

    bool postSensorReading(const SensorReading& reading)
    {
        JsonDocument document;
        document["satelliteId"] = Config::SatelliteId;
        document["temperatureCelsius"] = reading.temperatureCelsius;
        document["radiationLevel"] = reading.radiationLevel;
        document["batteryVoltage"] = reading.batteryVoltage;
        document["simulatedGravity"] = reading.simulatedGravity;
        document["simulatedThrust"] = reading.simulatedThrust;

        String payload;
        serializeJson(document, payload);

        String response;
        const bool ok = network.postJson(baseUrl() + "/api/satellite/sensor-readings", payload, response);
        if (!ok)
        {
            Serial.println("Failed to post sensor reading.");
            Serial.println(response);
        }

        return ok;
    }

    bool postManeuver(const ManeuverCommand& maneuver)
    {
        JsonDocument document;
        document["satelliteId"] = maneuver.satelliteId;
        document["conjunctionEventId"] = nullptr;
        document["action"] = maneuver.action;
        document["servoAngle"] = maneuver.servoAngle;
        document["thrustLevel"] = maneuver.thrustLevel;
        document["source"] = maneuver.source;

        String payload;
        serializeJson(document, payload);

        String response;
        const bool ok = network.postJson(baseUrl() + "/api/satellite/maneuver", payload, response);
        if (!ok)
        {
            Serial.println("Failed to post maneuver.");
            Serial.println(response);
        }

        return ok;
    }

private:
    OrbitShieldHttpClient& network;

    static String baseUrl()
    {
        String url = Config::ApiBaseUrl;
        url.trim();

        if (url.endsWith("/"))
        {
            url.remove(url.length() - 1);
        }

        return url;
    }

    static OrbitalVector readVector(JsonVariantConst variant)
    {
        OrbitalVector vector;
        vector.x = variant["x"] | 0.0f;
        vector.y = variant["y"] | 0.0f;
        vector.z = variant["z"] | 0.0f;
        return vector;
    }
};
