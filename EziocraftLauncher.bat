@echo off
setlocal enabledelayedexpansion

set "ZIP_URL=https://example.com/eziocraft.zip"
set "INSTALL_DIR=%LOCALAPPDATA%\Eziocraft"
set "ZIP_PATH=%INSTALL_DIR%\eziocraft.zip"
set "EXE_NAME=Eziocraft.exe"

if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

echo Téléchargement du zip...

powershell -NoProfile -ExecutionPolicy Bypass -Command "
$ErrorActionPreference='Stop';
$url='%ZIP_URL%';
$dest='%ZIP_PATH%';
$dir='%INSTALL_DIR%';
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null };
Invoke-WebRequest -Uri $url -OutFile $dest;
Add-Type -AssemblyName System.IO.Compression.FileSystem;
[System.IO.Compression.ZipFile]::ExtractToDirectory($dest, $dir, $true);
"
if errorlevel 1 (
    echo Erreur pendant le téléchargement ou l'extraction.
    pause
    exit /b 1
)

echo Lancement du jeu...

powershell -NoProfile -ExecutionPolicy Bypass -Command "
$ErrorActionPreference='Stop';
$root='%INSTALL_DIR%';
$exe=Get-ChildItem -Path $root -Filter '%EXE_NAME%' -File -Recurse | Select-Object -First 1;
if ($null -eq $exe) { Write-Host 'Executable introuvable.'; exit 1 };
Start-Process -FilePath $exe.FullName;
"

exit /b 0
