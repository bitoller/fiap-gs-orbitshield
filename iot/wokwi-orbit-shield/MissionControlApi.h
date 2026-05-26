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
};
