param(
    [switch]$Install
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$androidProject = Join-Path $root "android\OrbitShieldEngineer"
$sdkPath = Join-Path $env:LOCALAPPDATA "Android\Sdk"

if (-not (Test-Path $androidProject)) {
    throw "Android project not found at $androidProject"
}

if (-not (Test-Path $sdkPath)) {
    throw "Android SDK not found. Install Android Studio or set ANDROID_HOME manually."
}

$env:ANDROID_HOME = $sdkPath
$env:ANDROID_SDK_ROOT = $sdkPath

$localProperties = Join-Path $androidProject "local.properties"
$escapedSdkPath = $sdkPath.Replace("\", "\\")
"sdk.dir=$escapedSdkPath" | Set-Content -Path $localProperties -Encoding ASCII

Set-Location $androidProject
.\gradlew.bat :app:assembleDebug

$apkPath = Join-Path $androidProject "app\build\outputs\apk\debug\app-debug.apk"
Write-Host ""
Write-Host "APK generated:"
Write-Host "  $apkPath"

if ($Install) {
    $adb = Join-Path $sdkPath "platform-tools\adb.exe"
    if (-not (Test-Path $adb)) {
        throw "adb.exe not found. Install Android SDK Platform Tools from Android Studio."
    }

    $devices = & $adb devices | Select-String -Pattern "device$"
    if (-not $devices) {
        throw "No Android emulator or physical device detected. Start an emulator or connect a phone, then run again."
    }

    & $adb install -r $apkPath
    Write-Host "App installed on the connected Android device."
}
