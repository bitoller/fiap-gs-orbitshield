#pragma once

namespace Config
{
    constexpr const char* WifiSsid = "Wokwi-GUEST";
    constexpr const char* WifiPassword = "";

    // Wokwi ESP32 is more reliable with plain HTTP tunnels.
    constexpr const char* ApiBaseUrl = "http://cold-falcons-guess.loca.lt";

    constexpr int PollingIntervalMs = 5000;
    constexpr int HttpTimeoutMs = 8000;

    constexpr int SatelliteId = 1;
    constexpr int ServoPin = 18;
    constexpr int DhtPin = 15;
    constexpr int PotentiometerPin = 34;

    constexpr int ServoNominalAngle = 0;
    constexpr int ServoAvoidanceAngle = 90;

    constexpr float SimulatedGravity = 8.6942f;
    constexpr float BatteryVoltage = 4.10f;
}
