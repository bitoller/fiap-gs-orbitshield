#pragma once

#include <Arduino.h>
#include <ESP32Servo.h>
#include <LiquidCrystal_I2C.h>
#include <Wire.h>
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

        Wire.begin(Config::LcdSdaPin, Config::LcdSclPin);
        lcd.init();
        lcd.backlight();
        runStartupSelfTest();
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

    void triggerAutonomousAvoidance(const AutonomyDecision& decision, float thrustLevel)
    {
        servo.write(Config::ServoNominalAngle);
        delay(150);
        servo.write(Config::ServoAvoidanceAngle);

        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("AUTO AVOIDANCE");
        lcd.setCursor(0, 1);
        lcd.print("MISS:");
        lcd.print(decision.missDistanceKm, 1);
        lcd.print("KM");

        Serial.println("Autonomous collision risk detected onboard.");
        Serial.print("Time to closest approach: ");
        Serial.println(decision.timeToClosestApproachSeconds);
        Serial.print("Miss distance km: ");
        Serial.println(decision.missDistanceKm);
        Serial.print("Relative speed km/s: ");
        Serial.println(decision.relativeSpeedKmS);
        Serial.print("Thrust level: ");
        Serial.println(thrustLevel);
    }

private:
    void runStartupSelfTest()
    {
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("ACTUATOR TEST");
        lcd.setCursor(0, 1);
        lcd.print("SERVO + LCD OK");

        servo.write(Config::ServoNominalAngle);
        delay(400);
        servo.write(Config::ServoAvoidanceAngle);
        delay(500);
        servo.write(Config::ServoNominalAngle);
        delay(400);
    }

    Servo servo;
    LiquidCrystal_I2C lcd;
};
