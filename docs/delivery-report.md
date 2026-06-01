# Orbit Shield - Relatório Final de Entrega

## Integrantes

- Bianca Toller - RM553134
- Bruno Marcelino - RM553314

## Resumo do MVP

Orbit Shield é um MVP acadêmico de gestão autônoma de tráfego orbital. A prova de conceito integra backend, banco de dados, aplicativo mobile nativo e simulação IoT para demonstrar como um satélite poderia reduzir o atraso de reação em cenários de risco orbital.

O backend atua como Mission Control: persiste dados em Oracle, expõe API RESTful, autentica usuários, busca TLE público, propaga órbita com SGP4 e gera cenários de detritos. O ESP32 no Wokwi representa o computador embarcado do satélite: lê o ambiente orbital, calcula aproximação localmente, decide se deve manobrar e registra a ação. O app Android representa o painel do engenheiro: consome a API real, exibe telemetria, riscos, sensores e logs de manobras.

## 1. Banco de Dados

### Entregáveis

- Diagrama ER com mais de 4 entidades.
- Scripts SQL com `CREATE TABLE`, chaves primárias, chaves estrangeiras, constraints e índices.
- Consultas SQL básicas para simulação de uso.

### Evidências no projeto

- Diagrama ER: `docs/database-er-diagram.md`
- Script Oracle: `database/oracle/01_create_tables_current_user.sql`
- Script Oracle com schema local: `database/oracle/01_create_schema.sql`
- Consultas: `database/oracle/02_sample_queries.sql`

### Modelo relacional

O modelo usa tabelas com prefixo `OS_`, seguindo o padrão Orbit Shield:

- `OS_USERS`: usuários autenticados e perfis de acesso.
- `OS_SATELLITES`: satélites monitorados.
- `OS_DEBRIS_OBJECTS`: objetos de detrito orbital.
- `OS_ORBITAL_ELEMENTS`: TLEs e elementos orbitais.
- `OS_CONJUNCTION_EVENTS`: eventos de conjunção/risco.
- `OS_MANEUVER_LOGS`: manobras executadas pelo ESP32.
- `OS_SENSOR_READINGS`: telemetria enviada pelos sensores simulados.

O banco inclui constraints de domínio, unicidade, integridade referencial e validação de percentuais, ângulos e níveis de risco.

## 2. API Backend

### Entregáveis

- API RESTful em C# / .NET 8.
- Pelo menos 5 endpoints funcionais com GET, POST, PUT e DELETE.
- Organização em camadas: Controller, Service e Repository.
- Documentação via Swagger.

### Evidências no projeto

- Solução: `OrbitShield.sln`
- Controllers: `src/OrbitShield.Api/Controllers`
- Camada de aplicação: `src/OrbitShield.Application`
- Camada de infraestrutura/repositórios: `src/OrbitShield.Infrastructure`
- Documentação: `docs/api-endpoints.md`
- Swagger: `http://localhost:5184/swagger`

### Endpoints principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/satellites`
- `POST /api/satellites`
- `PUT /api/satellites/{id}`
- `DELETE /api/satellites/{id}`
- `GET /api/satellite/telemetry`
- `POST /api/satellite/sensor-readings`
- `POST /api/satellite/maneuver`
- `GET /api/orbital-scenarios/satellites/1/environment`
- `POST /api/orbital-scenarios/satellites/1/throw-random-debris`

## 3. Testes e Evidências

### Entregáveis

- Plano com pelo menos 5 casos de teste.
- Execução de pelo menos 3 testes.
- Evidências por prints, logs ou relatórios.

### Evidências no projeto

- Plano de testes backend: `docs/backend-test-plan.md`
- Evidência IoT: `docs/iot-test-plan.md`
- Build Android validado: `powershell -ExecutionPolicy Bypass -File .\scripts\build-android.ps1`
- Inicialização integrada: `powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1`

### Casos cobertos

O plano registra 7 cenários: telemetria, conjunção crítica, sensor IoT, manobra ESP32, autenticação, endpoint protegido por JWT e armazenamento de TLE.

### O que são os logs

Logs são saídas de execução que comprovam comportamento do sistema. No projeto, eles aparecem em três lugares:

- Backend/Docker: `docker logs orbitshield-api --tail 80`
- Túnel público: `docker logs orbitshield-tunnel --tail 50`
- Wokwi: Serial Monitor da simulação ESP32

Exemplos de evidência:

- `POST /api/satellite/sensor-readings -> 201`
- `GET /api/orbital-scenarios/satellites/1/environment -> 200`
- `Autonomous collision risk detected onboard.`
- `POST /api/satellite/maneuver -> 200`
- `BUILD SUCCESSFUL`

Esses logs mostram que sensores enviam dados, o backend responde, o ESP32 calcula risco autonomamente e a manobra é persistida.

## 4. Mobile Android

### Entregáveis

- Protótipo funcional com no mínimo 3 telas.
- Layout responsivo e usável.
- Integração com a API.

### Evidências no projeto

- App Android: `android/OrbitShieldEngineer`
- Guia: `docs/android-setup.md`
- Gradle Wrapper: `android/OrbitShieldEngineer/gradlew.bat`

### Telas implementadas

- Login
- Dashboard
- Maneuver Logs
- Global / Scenario Control
- Settings / System Status

O app usa Kotlin, Jetpack Compose, Material 3, Retrofit, Gson, OkHttp, Coroutines, StateFlow e DataStore. A arquitetura segue MVVM com separação entre `data`, `domain`, `presentation` e `ui`.

O app não usa dados mockados: as telas consomem endpoints reais do backend.

## 5. Segurança

### Entregáveis

- Login com senha criptografada.
- Controle de acesso por perfil.
- Pelo menos 2 práticas de segurança.

### Implementação

- Senhas armazenadas com BCrypt.
- Autenticação JWT Bearer.
- Perfis `Admin`, `Engineer` e `SatelliteDevice`.
- Endpoints administrativos protegidos por autorização.
- DTOs e validações de entrada.
- EF Core com consultas parametrizadas, reduzindo risco de SQL Injection.
- Segredos locais protegidos por `.gitignore` e exemplos seguros em `.env.example`.

## 6. IoT / Wokwi

### Entregáveis

- Simulação/código de integração com sensores.
- Dados enviados para aplicação.
- Explicação da lógica do sensor e sua influência.

### Evidências no projeto

- Código Wokwi: `iot/wokwi-orbit-shield`
- Evidência: `docs/iot-test-plan.md`
- Modelo orbital: `docs/orbital-autonomy.md`

### Componentes simulados

- ESP32: computador embarcado do satélite.
- DHT22: telemetria térmica.
- Potenciômetro: propulsão/thrust simulado.
- LCD I2C: estado da missão para o público.
- LED vermelho: alerta visual de risco.
- Servo motor: atuador visual de manobra evasiva.

### Lógica autônoma

O backend fornece vetores orbitais e cenários de detritos. O ESP32 calcula localmente:

```text
tca = -dot(relativePosition, relativeVelocity) / |relativeVelocity|^2
missDistance = |relativePosition + relativeVelocity * tca|
risk = missDistance <= safeDistance
```

Se houver risco, o ESP32 calcula o ângulo de manobra, move o servo, acende o LED e envia o log para o backend. Isso demonstra a premissa do projeto: reduzir lag evitando depender de um comando manual do Mission Control.

## 7. Execução do Projeto

### Comando principal

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

Esse script sobe Oracle, backend e túnel público no Docker, prepara o usuário demo e reseta a missão para `SAFE PASS`.

### Android

Abra `android/OrbitShieldEngineer` no Android Studio e use:

```text
API URL: http://10.0.2.2:5184
email: orbit.demo.2026@orbitshield.local
password: OrbitShield123!
```

### Wokwi

Abra um projeto ESP32 Arduino no Wokwi e copie os arquivos de `iot/wokwi-orbit-shield`. Atualize `Config.h` com a URL pública mostrada pelo script.

## Conclusão

O Orbit Shield atende aos requisitos técnicos do desafio: arquitetura distribuída, banco relacional, API RESTful, autenticação, app mobile nativo, simulação IoT e evidências de teste. O MVP demonstra uma prova de conceito interdisciplinar em que o satélite simulado recebe dados orbitais, calcula risco localmente, executa manobra autônoma e reporta o resultado para Mission Control.
