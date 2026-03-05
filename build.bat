@echo off
REM Build script for Windows

echo Building AMSI.fail CLI for Windows (x64)...
echo.

dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo BUILD SUCCESSFUL
    echo ========================================
    echo.
    echo Executable location:
    echo bin\Release\net6.0\win-x64\publish\AMSIBypass.exe
    echo.
    echo You can now run the executable from the publish directory.
) else (
    echo.
    echo ========================================
    echo BUILD FAILED
    echo ========================================
    echo.
    echo Please ensure .NET 6.0 SDK or later is installed.
    echo Download from: https://dotnet.microsoft.com/download
)

pause
