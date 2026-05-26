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
        pinMode(Config::AlertLedPin, OUTPUT);
        digitalWrite(Config::AlertLedPin, LOW);

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
        digitalWrite(Config::AlertLedPin, LOW);
        servo.write(Config::ServoNominalAngle);
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("ORBIT SHIELD");
        lcd.setCursor(0, 1);
        lcd.print("SCANNING ORBIT");
    }

    void showTelemetry(const SensorReading& reading)
    {
        Serial.print("Temperature C: ");
        Serial.println(reading.temperatureCelsius);
        Serial.print("Radiation level: ");
        Serial.println(reading.radiationLevel);
        Serial.print("Battery voltage: ");
        Serial.println(reading.batteryVoltage);
        Serial.print("Simulated thrust: ");
        Serial.println(reading.simulatedThrust);
    }

    void triggerAvoidance(const ConjunctionAlert& alert, float thrustLevel)
    {
        digitalWrite(Config::AlertLedPin, HIGH);
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
        digitalWrite(Config::AlertLedPin, HIGH);
        servo.write(Config::ServoNominalAngle);
        delay(150);
        servo.write(decision.servoAngle);

        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print(decision.predictedImpact ? "IMPACT RISK" : "AUTO AVOIDANCE");
        lcd.setCursor(0, 1);
        lcd.print("MISS ");
        lcd.print(decision.missDistanceKm, 1);
        lcd.print(" A");
        lcd.print(decision.servoAngle);

        if (decision.predictedImpact)
        {
            Serial.println("Impact predicted: avoidance attempted too late.");
        }
        Serial.println("Autonomous collision risk detected onboard.");
        Serial.print("Time to closest approach: ");
        Serial.println(decision.timeToClosestApproachSeconds);
        Serial.print("Miss distance km: ");
        Serial.println(decision.missDistanceKm);
        Serial.print("Relative speed km/s: ");
        Serial.println(decision.relativeSpeedKmS);
        Serial.print("Servo angle: ");
        Serial.println(decision.servoAngle);
        Serial.print("Thrust level: ");
        Serial.println(thrustLevel);
    }

    void showSafePass(const AutonomyDecision& decision)
    {
        digitalWrite(Config::AlertLedPin, decision.alertLedOn ? HIGH : LOW);
        servo.write(Config::ServoNominalAngle);

        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print(safeTitle(decision));
        lcd.setCursor(0, 1);
        lcd.print(safeSubtitle(decision));

        Serial.println("No avoidance required.");
        Serial.print("Miss distance km: ");
        Serial.println(decision.missDistanceKm);
    }

private:
    static String safeTitle(const AutonomyDecision& decision)
    {
        if (decision.riskLabel == "SAFE PASS")
        {
            return "SAFE PASS";
        }

        if (decision.riskLabel == "NEAR MISS")
        {
            return "TRACKING DEBRIS";
        }

        return "SCANNING ORBIT";
    }

    static String safeSubtitle(const AutonomyDecision& decision)
    {
        if (decision.riskLabel == "SAFE PASS")
        {
            return "OBJECT PASSED";
        }

        if (decision.riskLabel == "NEAR MISS")
        {
            return "WAITING PASS";
        }

        return "NO THREATS";
    }

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
