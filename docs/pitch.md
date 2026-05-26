# Orbit Shield Pitch

## Core Idea

Orbit Shield is an autonomous orbital traffic management MVP.

The project does not try to compete on propulsion hardware. Chemical propulsion, electric propulsion, reaction wheels and magnetorquers are different ways to move a spacecraft. Orbit Shield focuses on the harder software question:

```text
How does the satellite decide, fast enough, that it must maneuver?
```

The project is about onboard autonomy. The ESP32 in Wokwi represents the satellite onboard computer installed inside the spacecraft. It receives orbital environment data, calculates collision risk locally and decides whether to maneuver without waiting for a human command from Earth.

## One-Sentence Pitch

Orbit Shield is the onboard decision layer that allows a satellite to detect collision risk, calculate closest approach and execute an avoidance maneuver autonomously, reducing dependence on delayed ground commands.

## What Each Layer Represents

```text
Backend API
Mission Control / orbital environment simulator
Fetches TLE data, propagates orbit with SGP4 and injects debris scenarios.

ESP32 Wokwi
Onboard satellite computer
Receives orbital vectors, calculates closest approach locally and decides whether to maneuver.

Servo motor
Visible avoidance actuator
Represents thrusters, reaction wheels or another real spacecraft maneuvering system.

LCD + red LED
Mission status panel
Shows scanning, tracking, avoidance, impact risk and safe pass states.

Android app
Engineer panel
Will monitor telemetry, alerts and maneuver history.
```

## Where The Satellite "Sees" Debris

In this MVP, Wokwi does not simulate a real radar or optical telescope. The debris is perceived through orbital environment data returned by the backend:

```text
relativePositionKm
relativeVelocityKmS
debrisDiameterMeters
estimatedMassKg
impactEnergyJoules
debrisClass
scenarioClassification
predictedImpact
```

The backend plays the role of a free academic version of a space situational awareness layer. It uses public TLE data and SGP4 propagation, then creates debris trajectories for testing.

In a real spacecraft, these inputs could come from:

```text
ground radar
optical tracking
space surveillance catalogs
TLE/ephemeris updates
onboard optical sensors
inter-satellite warnings
```

The important architectural point is that Mission Control provides awareness, but the satellite owns the final maneuver decision.

## Why This Reduces Lag

Traditional flow:

```text
Detect object on Earth
Analyze risk on Earth
Send command to satellite
Satellite waits for instruction
Satellite maneuvers
```

Orbit Shield flow:

```text
Orbital environment data reaches the satellite
Satellite calculates closest approach onboard
Satellite decides locally
Satellite maneuvers immediately
Satellite reports the maneuver back to Mission Control
```

This reduces the dangerous delay between risk detection and action. The backend does not tell the ESP32 "move the servo." The ESP32 calculates:

```text
tca = -dot(relativePosition, relativeVelocity) / |relativeVelocity|^2
missDistance = |relativePosition + relativeVelocity * tca|
risk = missDistance <= safeDistance
```

Then it determines the maneuver intensity:

```text
servoAngle = 25 + distanceSeverity * 40 + urgency * 15 + thrustAuthority * 10
```

So the onboard layer decides:

```text
SAFE PASS
NEAR MISS
AVOIDANCE REQUIRED
IMPACT PREDICTED
```

## What Happens In The Demo

1. The backend starts with Docker and exposes Swagger.
2. Wokwi runs the ESP32 satellite simulator.
3. Swagger throws a random debris object with size, mass and trajectory.
4. The backend updates the debris relative position over simulated time.
5. The ESP32 polls the environment every few seconds.
6. The ESP32 calculates closest approach locally.
7. If the object is dangerous, the red LED turns on and the servo maneuvers.
8. If the object passes safely, the LCD shows `SAFE PASS` and the servo returns to nominal.

The LCD is designed for the audience:

```text
SCANNING ORBIT
TRACKING DEBRIS
AUTO AVOIDANCE
IMPACT RISK
SAFE PASS
```

Raw telemetry stays in the Serial Monitor logs.

## The Strong Argument

While many solutions focus on the propulsion mechanism, Orbit Shield focuses on the decision bottleneck.

It does not matter whether a satellite uses chemical propulsion, electric propulsion, reaction wheels or magnetorquers if it cannot decide fast enough when to use them.

Orbit Shield is propulsion-agnostic. It is the autonomous software layer that evaluates orbital risk and triggers the appropriate maneuver locally, reducing human dependency and communication latency between Earth and space.

## Why This Is A Software Engineering Project

Orbit Shield combines:

- RESTful backend in .NET 8.
- Oracle database with `OS_*` relational tables.
- Security with JWT, BCrypt and role-based access control.
- Public TLE usage and SGP4 orbit propagation.
- IoT simulation with ESP32 in Wokwi.
- Onboard autonomy logic.
- Future Android engineer dashboard with Kotlin and Jetpack Compose.

The value is in distributed system design:

```text
Mission awareness -> onboard autonomy -> actuator response -> telemetry feedback
```

## Honest MVP Limitation

The ESP32 does not contain a real radar. In the MVP, the backend simulates the orbital awareness layer and sends orbital vectors to the onboard computer.

This is acceptable for the prototype because the goal is to prove the autonomy architecture:

```text
The satellite receives environment data.
The satellite calculates risk locally.
The satellite acts without waiting for a ground command.
```

In a production spacecraft, the same onboard logic would receive inputs from real tracking systems and flight-certified sensors.

---

# Pitch Do Orbit Shield

## Ideia Central

Orbit Shield é um MVP de gestão autônoma de tráfego orbital.

O projeto não tenta competir em hardware de propulsão. Propulsão química, propulsão elétrica, rodas de reação e magnetorquers são formas diferentes de mover uma nave ou satélite. O Orbit Shield foca em uma pergunta de software mais difícil:

```text
Como o satélite decide, rápido o suficiente, que precisa manobrar?
```

O projeto é sobre autonomia embarcada. O ESP32 no Wokwi representa o computador de bordo instalado dentro do satélite. Ele recebe dados do ambiente orbital, calcula localmente o risco de colisão e decide se deve manobrar sem esperar um comando humano vindo da Terra.

## Pitch Em Uma Frase

Orbit Shield é a camada de decisão embarcada que permite a um satélite detectar risco de colisão, calcular maior aproximação e executar uma manobra evasiva de forma autônoma, reduzindo a dependência de comandos terrestres atrasados.

## O Que Cada Camada Representa

```text
Backend API
Mission Control / simulador de ambiente orbital
Busca dados TLE, propaga a órbita com SGP4 e injeta cenários de detritos.

ESP32 Wokwi
Computador de bordo do satélite
Recebe vetores orbitais, calcula closest approach localmente e decide se deve manobrar.

Servo motor
Atuador visual de desvio
Representa thrusters, rodas de reação ou outro sistema real de manobra espacial.

LCD + LED vermelho
Painel de status da missão
Mostra varredura, rastreamento, evasão, risco de impacto e passagem segura.

App Android
Painel do engenheiro
Vai monitorar telemetria, alertas e histórico de manobras.
```

## Por Onde O Satélite "Enxerga" Os Debris

Neste MVP, o Wokwi não simula um radar real ou telescópio óptico. O detrito é percebido por dados de ambiente orbital retornados pelo backend:

```text
relativePositionKm
relativeVelocityKmS
debrisDiameterMeters
estimatedMassKg
impactEnergyJoules
debrisClass
scenarioClassification
predictedImpact
```

O backend faz o papel de uma versão acadêmica gratuita de uma camada de consciência situacional espacial. Ele usa dados públicos TLE e propagação SGP4, depois cria trajetórias de detritos para teste.

Em um satélite real, esses dados poderiam vir de:

```text
radares terrestres
rastreamento óptico
catálogos de vigilância espacial
atualizações TLE/efemérides
sensores ópticos embarcados
alertas entre satélites
```

O ponto arquitetural mais importante é: o Mission Control fornece consciência do ambiente, mas o satélite toma a decisão final de manobra.

## Por Que Isso Reduz Lag

Fluxo tradicional:

```text
Detecta objeto na Terra
Analisa risco na Terra
Envia comando para o satélite
Satélite espera instrução
Satélite manobra
```

Fluxo do Orbit Shield:

```text
Dados do ambiente orbital chegam ao satélite
Satélite calcula closest approach a bordo
Satélite decide localmente
Satélite manobra imediatamente
Satélite reporta a manobra para o Mission Control
```

Isso reduz o atraso perigoso entre a detecção do risco e a ação. O backend não diz ao ESP32 "mova o servo". O ESP32 calcula:

```text
tca = -dot(relativePosition, relativeVelocity) / |relativeVelocity|^2
missDistance = |relativePosition + relativeVelocity * tca|
risk = missDistance <= safeDistance
```

Depois ele determina a intensidade da manobra:

```text
servoAngle = 25 + distanceSeverity * 40 + urgency * 15 + thrustAuthority * 10
```

Assim, a camada embarcada decide:

```text
SAFE PASS
NEAR MISS
AVOIDANCE REQUIRED
IMPACT PREDICTED
```

## O Que Acontece Na Demo

1. O backend sobe com Docker e expõe o Swagger.
2. O Wokwi roda o simulador do satélite com ESP32.
3. O Swagger lança um debris aleatório com tamanho, massa e trajetória.
4. O backend atualiza a posição relativa do debris ao longo do tempo simulado.
5. O ESP32 consulta o ambiente a cada poucos segundos.
6. O ESP32 calcula localmente a maior aproximação.
7. Se o objeto for perigoso, o LED vermelho acende e o servo manobra.
8. Se o objeto passar com segurança, o LCD mostra `SAFE PASS` e o servo volta ao nominal.

O LCD foi pensado para o público:

```text
SCANNING ORBIT
TRACKING DEBRIS
AUTO AVOIDANCE
IMPACT RISK
SAFE PASS
```

A telemetria bruta fica nos logs do Serial Monitor.

## O Argumento Forte

Enquanto muitas soluções focam no mecanismo de propulsão, o Orbit Shield foca no gargalo de decisão.

Não importa se um satélite usa propulsão química, propulsão elétrica, rodas de reação ou magnetorquers se ele não consegue decidir rápido o suficiente quando usar esses sistemas.

Orbit Shield é agnóstico à propulsão. Ele é a camada autônoma de software que avalia risco orbital e aciona a manobra apropriada localmente, reduzindo dependência humana e latência de comunicação entre Terra e espaço.

## Por Que Isso É Um Projeto De Engenharia De Software

Orbit Shield combina:

- Backend RESTful em .NET 8.
- Banco Oracle com tabelas relacionais `OS_*`.
- Segurança com JWT, BCrypt e controle de acesso por perfil.
- Uso de TLE público e propagação orbital SGP4.
- Simulação IoT com ESP32 no Wokwi.
- Lógica de autonomia embarcada.
- Futuro painel Android com Kotlin e Jetpack Compose.

O valor está no desenho de sistemas distribuídos:

```text
Consciência de missão -> autonomia embarcada -> resposta do atuador -> feedback de telemetria
```

## Limitação Honesta Do MVP

O ESP32 não contém um radar real. No MVP, o backend simula a camada de consciência orbital e envia vetores orbitais para o computador de bordo.

Isso é aceitável para o protótipo porque o objetivo é provar a arquitetura de autonomia:

```text
O satélite recebe dados do ambiente.
O satélite calcula risco localmente.
O satélite age sem esperar comando terrestre.
```

Em um satélite de produção, a mesma lógica embarcada receberia entradas de sistemas reais de rastreamento e sensores certificados para voo.
