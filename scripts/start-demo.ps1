param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$androidProject = Join-Path $root "android\OrbitShieldEngineer"
$sdkPath = Join-Path $env:LOCALAPPDATA "Android\Sdk"

Write-Host "Starting Orbit Shield backend, Oracle and public tunnel..."
Set-Location $root

if ($SkipBuild) {
    docker compose up -d
} else {
    docker compose up -d --build
}

Write-Host "Waiting for tunnel URL..."
$publicUrl = $null
for ($i = 0; $i -lt 30; $i++) {
    $logs = docker logs orbitshield-tunnel --tail 80 2>$null
    $match = $logs | Select-String -Pattern "https://[a-zA-Z0-9\-]+\.loca\.lt" | Select-Object -Last 1
    if ($match) {
        $publicUrl = $match.Matches[0].Value
        break
    }
    Start-Sleep -Seconds 2
}

Write-Host "Waiting for Mission Control API..."
$apiReady = $false
for ($i = 0; $i -lt 45; $i++) {
    try {
        Invoke-RestMethod -Uri "http://localhost:5184/api/satellite/telemetry" -Method Get | Out-Null
        $apiReady = $true
        break
    } catch {
        Start-Sleep -Seconds 2
    }
}

if (-not $apiReady) {
    throw "Mission Control API did not become ready at http://localhost:5184"
}

Write-Host "Preparing demo user and safe initial scenario..."
$registerBody = @{
    name = "Orbit Shield Demo"
    email = "orbit.demo.2026@orbitshield.local"
    password = "OrbitShield123!"
    role = "Engineer"
} | ConvertTo-Json

try {
    Invoke-RestMethod `
        -Uri "http://localhost:5184/api/auth/register" `
        -Method Post `
        -ContentType "application/json" `
        -Body $registerBody | Out-Null
} catch {
    Write-Host "Demo user already exists or could not be re-created. Continuing..."
}

try {
    Invoke-RestMethod `
        -Uri "http://localhost:5184/api/orbital-scenarios/satellites/1/trigger-preset?preset=SafePass" `
        -Method Post | Out-Null
} catch {
    Write-Host "Could not reset scenario automatically. You can use the SafePass preset in Swagger."
}

if (-not (Test-Path $androidProject)) {
    throw "Android project not found at $androidProject"
}

if (Test-Path $sdkPath) {
    $localProperties = Join-Path $androidProject "local.properties"
    $escapedSdkPath = $sdkPath.Replace("\", "\\")
    "sdk.dir=$escapedSdkPath" | Set-Content -Path $localProperties -Encoding ASCII
}

Write-Host ""
Write-Host "Orbit Shield is ready."
Write-Host ""
Write-Host "Swagger:"
Write-Host "  http://localhost:5184/swagger"
Write-Host ""
Write-Host "Android Emulator API URL:"
Write-Host "  http://10.0.2.2:5184"
Write-Host ""

if ($publicUrl) {
    $httpPublicUrl = $publicUrl.Replace("https://", "http://")
    Write-Host "Physical phone / Wokwi API URL:"
    Write-Host "  $httpPublicUrl"
    Write-Host ""
    Write-Host "Paste this URL in:"
    Write-Host "  Android login screen, when using a physical phone"
    Write-Host "  iot\wokwi-orbit-shield\Config.h, when using Wokwi"
} else {
    Write-Host "Tunnel URL was not detected yet."
    Write-Host "Run later:"
    Write-Host "  docker logs orbitshield-tunnel --tail 50"
}

Write-Host ""
Write-Host "Android project:"
Write-Host "  $androidProject"
Write-Host ""
Write-Host "Build Android:"
Write-Host "  powershell -ExecutionPolicy Bypass -File .\scripts\build-android.ps1"
