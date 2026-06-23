@echo off
title IOCL Community Hall — Web Forms Server
color 0F
cls

echo.
echo  ╔══════════════════════════════════════════════════════════╗
echo  ║     IOCL Community Hall — ASP.NET Web Forms Server      ║
echo  ║          Panipat Refinery Township Portal                ║
echo  ╚══════════════════════════════════════════════════════════╝
echo.

:: ── Set project path ──────────────────────────────────────────
set "PROJECT_DIR=%~dp0"
set "BIN_DIR=%PROJECT_DIR%bin"
set "PORT=5051"

:: ── Step 1: Check SQLite DLL in bin\... ──────────────────────
echo [1/3] Checking SQLite DLL in bin\...

if not exist "%BIN_DIR%" mkdir "%BIN_DIR%"

if not exist "%BIN_DIR%\System.Data.SQLite.dll" (
    echo  Downloading System.Data.SQLite via NuGet...
    
    :: Try to find nuget.exe
    set "NUGET="
    where nuget >nul 2>&1
    if %errorlevel%==0 set "NUGET=nuget"
    
    if not defined NUGET (
        if exist "%PROJECT_DIR%nuget.exe" set "NUGET=%PROJECT_DIR%nuget.exe"
    )
    
    if not defined NUGET (
        echo  Downloading nuget.exe...
        powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%PROJECT_DIR%nuget.exe'}" >nul 2>&1
        if exist "%PROJECT_DIR%nuget.exe" set "NUGET=%PROJECT_DIR%nuget.exe"
    )
    
    if defined NUGET (
        echo  Installing System.Data.SQLite package...
        "%NUGET%" install System.Data.SQLite -Version 1.0.118.0 -OutputDirectory "%PROJECT_DIR%packages" -NonInteractive >nul 2>&1
        
        :: Copy DLLs to bin
        for /r "%PROJECT_DIR%packages" %%f in (System.Data.SQLite.dll) do (
            if "%%~nxf"=="System.Data.SQLite.dll" copy "%%f" "%BIN_DIR%\" >nul 2>&1
        )
        for /r "%PROJECT_DIR%packages" %%f in (SQLite.Interop.dll) do (
            if "%%~nxf"=="SQLite.Interop.dll" copy "%%f" "%BIN_DIR%\" >nul 2>&1
        )
    ) else (
        echo  [WARNING] Could not download SQLite automatically.
        echo  Please manually copy System.Data.SQLite.dll to:
        echo  %BIN_DIR%\
    )
) else (
    echo  √ System.Data.SQLite.dll already present.
)

:: ── Step 2: Compile Custom Server if missing ──────────────────
echo.
echo [2/3] Checking server executable...

if exist "%BIN_DIR%\RunServer.exe" goto ServerExists

echo  Server executable missing. Attempting compilation...

set "VBC="
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\vbc.exe" set "VBC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\vbc.exe"
if not defined VBC if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\vbc.exe" set "VBC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\vbc.exe"

if not defined VBC (
    echo  [ERROR] VB.NET compiler (vbc.exe) not found.
    pause
    exit /b 1
)

echo  Using compiler: %VBC%
"%VBC%" /target:exe /out:"%BIN_DIR%\RunServer.exe" /r:System.Web.dll,System.dll,System.Core.dll WebHost.vb Server.vb
if not exist "%BIN_DIR%\RunServer.exe" (
    echo  [ERROR] Compilation failed.
    pause
    exit /b 1
)
echo  √ Compilation succeeded.

:ServerExists
echo  √ Server executable is present.

:: Ensure config is in bin
if not exist "%BIN_DIR%\RunServer.exe.config" (
    if exist "%PROJECT_DIR%RunServer.exe.config" (
        copy "%PROJECT_DIR%RunServer.exe.config" "%BIN_DIR%\" >nul
    )
)

:: ── Step 3: Launch Local Web Server ───────────────────────────
echo.
echo [3/3] Starting server...
echo.
echo  ┌─────────────────────────────────────────────────────────┐
echo  │                                                         │
echo  │   🌐  IOCL Portal running at:                          │
echo  │       http://localhost:%PORT%/Login.aspx               │
echo  │                                                         │
echo  │   👤  Default Login Credentials:                        │
echo  │       Employee ID : 00000001                            │
echo  │       Password    : Admin@123                           │
echo  │                                                         │
echo  │   Press Ctrl+C or close this window to stop.           │
echo  │                                                         │
echo  └─────────────────────────────────────────────────────────┘
echo.

:: Open browser after 2 second delay
start /b cmd /c "ping -n 3 127.0.0.1 >nul && start http://localhost:%PORT%/Login.aspx"

:: Run the server executable
"%BIN_DIR%\RunServer.exe"

echo.
echo  Server stopped.
pause
