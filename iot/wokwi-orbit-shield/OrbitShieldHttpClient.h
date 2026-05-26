#pragma once

#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include "Config.h"

class OrbitShieldHttpClient
{
public:
    void connectWifi()
    {
        WiFi.mode(WIFI_STA);
        WiFi.begin(Config::WifiSsid, Config::WifiPassword);

        Serial.print("Connecting to Wi-Fi");
        while (WiFi.status() != WL_CONNECTED)
        {
            delay(500);
            Serial.print(".");
        }

        Serial.println();
        Serial.print("Wi-Fi connected. IP: ");
        Serial.println(WiFi.localIP());
    }

    bool isConnected() const
    {
        return WiFi.status() == WL_CONNECTED;
    }

    bool get(const String& url, String& responseBody)
    {
        Serial.print("GET ");
        Serial.println(url);

        WiFiClient client;

        HTTPClient http;
        http.setTimeout(Config::HttpTimeoutMs);

        if (!http.begin(client, url))
        {
            Serial.println("HTTP begin failed.");
            return false;
        }

        addTunnelHeaders(http);
        const int statusCode = http.GET();
        responseBody = http.getString();
        http.end();

        Serial.print("GET status: ");
        Serial.println(statusCode);
        if (statusCode < 0)
        {
            Serial.print("GET error: ");
            Serial.println(http.errorToString(statusCode));
        }

        return statusCode >= 200 && statusCode < 300;
    }

    bool postJson(const String& url, const String& jsonBody, String& responseBody)
    {
        Serial.print("POST ");
        Serial.println(url);
        Serial.print("Payload: ");
        Serial.println(jsonBody);

        WiFiClient client;

        HTTPClient http;
        http.setTimeout(Config::HttpTimeoutMs);

        if (!http.begin(client, url))
        {
            Serial.println("HTTP begin failed.");
            return false;
        }

        addTunnelHeaders(http);
        http.addHeader("Content-Type", "application/json");

        const int statusCode = http.POST(jsonBody);
        responseBody = http.getString();
        http.end();

        Serial.print("POST status: ");
        Serial.println(statusCode);
        if (statusCode < 0)
        {
            Serial.print("POST error: ");
            Serial.println(http.errorToString(statusCode));
        }

        return statusCode >= 200 && statusCode < 300;
    }

private:
    static void addTunnelHeaders(HTTPClient& http)
    {
        http.addHeader("ngrok-skip-browser-warning", "true");
    }
};
