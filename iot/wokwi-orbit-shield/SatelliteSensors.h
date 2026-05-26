#pragma once

#include <Arduino.h>
#include <DHT.h>
#include "Config.h"
#include "Models.h"

class SatelliteSensors
{
public:
    SatelliteSensors() : dht(Config::DhtPin, DHT22)
    {
    }

    void begin()
    {
        dht.begin();
        pinMode(Config::PotentiometerPin, INPUT);
    }

    SensorReading read()
    {
        SensorReading reading;
        reading.temperatureCelsius = readTemperature();
        reading.simulatedThrust = readThrustLevel();
        reading.radiationLevel = calculateRadiation(reading.simulatedThrust);
        reading.batteryVoltage = Config::BatteryVoltage;
        reading.simulatedGravity = Config::SimulatedGravity;

        return reading;
    }

private:
    DHT dht;

    float readTemperature()
    {
        const float temperature = dht.readTemperature();
        return isnan(temperature) ? 24.0f : temperature;
    }

    static float readThrustLevel()
    {
        const int rawValue = analogRead(Config::PotentiometerPin);
        return map(rawValue, 0, 4095, 0, 100);
    }

    static float calculateRadiation(float thrustLevel)
    {
        return 8.0f + (thrustLevel * 0.12f);
    }
};
