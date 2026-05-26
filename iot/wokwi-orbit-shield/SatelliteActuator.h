#pragma once

#include <Arduino.h>
#include <ESP32Servo.h>
#include <LiquidCrystal_I2C.h>
#include "Config.h"
#include "Models.h"

class SatelliteActuator
{
public:
    SatelliteActuator() : lcd(0x27, 16, 2)
    {
    }

    void begin()
    {
        servo.setPeriodHertz(50);
        servo.attach(Config::ServoPin, 500, 2400);
        servo.write(Config::ServoNominalAngle);

        lcd.init();
        lcd.backlight();
        showNominal();
    }

    void showNominal()
    {
        servo.write(Config::ServoNominalAngle);
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("ORBIT SHIELD");
        lcd.setCursor(0, 1);
        lcd.print("STATUS: NOMINAL");
    }

    void showTelemetry(const SensorReading& reading)
    {
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("TEMP:");
        lcd.print(reading.temperatureCelsius, 1);
        lcd.print("C");
        lcd.setCursor(0, 1);
        lcd.print("THRUST:");
        lcd.print(reading.simulatedThrust, 0);
        lcd.print("%");
    }

    void triggerAvoidance(const ConjunctionAlert& alert, float thrustLevel)
    {
        servo.write(Config::ServoAvoidanceAngle);

        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("COLLISION ALERT");
        lcd.setCursor(0, 1);
        lcd.print(alert.debrisId.substring(0, 16));

        Serial.println("Collision alert received.");
        Serial.print("Debris: ");
        Serial.println(alert.debrisId);
        Serial.print("Thrust level: ");
        Serial.println(thrustLevel);
    }

private:
    Servo servo;
    LiquidCrystal_I2C lcd;
};
