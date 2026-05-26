# Orbit Shield MVP

## Summary

Build an academic full-stack MVP for autonomous orbital traffic management using:

- .NET 8 REST API as Mission Control
- Oracle Database with relational `OS_*` tables
- ESP32 Wokwi simulation as the satellite
- Android Kotlin app as the engineer panel

## Goals

- Provide a functional REST backend with clean layering.
- Persist mission data in Oracle with constraints and relationships.
- Simulate collision avoidance through Wokwi ESP32.
- Prepare data contracts for the Android MVVM app.
- Include security practices: BCrypt, JWT, roles, validation and EF Core parameterization.

## Non-Goals

- Real orbital propagation embedded in ESP32.
- Real spacecraft hardware control.
- Production deployment or paid hosting.

## Current Status

Backend and IoT integration are functional. Android implementation is intentionally postponed.
