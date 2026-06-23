@echo off
title IOCL — Setup Dependencies
color 0B
cls

echo.
echo  ╔══════════════════════════════════════════════════════════╗
echo  ║       IOCL Community Hall — Setup Dependencies          ║
echo  ╚══════════════════════════════════════════════════════════╝
echo.

set "PROJECT_DIR=%~dp0"
set "BIN_DIR=%PROJECT_DIR%bin"
set "PKG_DIR=%PROJECT_DIR%packages"

if not exist "%BIN_DIR%" mkdir "%BIN_DIR%"
if not exist "%PKG_DIR%" mkdir "%PKG_DIR%"

:: ── Download NuGet ────────────────────────────────────────────
echo [1/3] Checking NuGet...
if not exist "%PROJECT_DIR%nuget.exe" (
    echo  Downloading nuget.exe...
    powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%PROJECT_DIR%nuget.exe'}"
    if exist "%PROJECT_DIR%nuget.exe" (
        echo  √ nuget.exe downloaded.
    ) else (
        echo  [ERROR] Failed to download nuget.exe. Check internet connection.
        pause
        exit /b 1
    )
) else (
    echo  √ nuget.exe already present.
)

:: ── Install SQLite ────────────────────────────────────────────
echo.
echo [2/3] Installing System.Data.SQLite...
"%PROJECT_DIR%nuget.exe" install System.Data.SQLite -Version 1.0.118.0 -OutputDirectory "%PKG_DIR%" -NonInteractive
if %errorlevel% neq 0 (
    echo  [WARNING] NuGet install returned an error (may already be installed).
)

:: Copy DLLs to bin
echo  Copying DLLs to bin\...
for /r "%PKG_DIR%" %%f in (System.Data.SQLite.dll) do (
    if "%%~nxf"=="System.Data.SQLite.dll" (
        copy /y "%%f" "%BIN_DIR%\" >nul
        echo  √ Copied: %%~nxf
    )
)
for /r "%PKG_DIR%" %%f in (SQLite.Interop.dll) do (
    if "%%~nxf"=="SQLite.Interop.dll" (
        copy /y "%%f" "%BIN_DIR%\" >nul
        echo  √ Copied: %%~nxf
    )
)

:: ── Verify ────────────────────────────────────────────────────
echo.
echo [3/3] Verifying setup...
if exist "%BIN_DIR%\System.Data.SQLite.dll" (
    echo  √ System.Data.SQLite.dll  — OK
) else (
    echo  ✗ System.Data.SQLite.dll  — MISSING (run as Administrator or copy manually)
)

echo.
echo  ═══════════════════════════════════════════
echo   Setup complete! Now double-click run.bat
echo  ═══════════════════════════════════════════
echo.
pause
