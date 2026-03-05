@echo off
REM Simple build script - creates standalone EXE (NO .NET REQUIRED ON TARGET PC)

echo ========================================
echo AMSI.fail CLI - Building Standalone EXE
echo ========================================
echo.

REM Clean previous builds
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo Cleaning old builds...
echo Building standalone executable (includes .NET runtime)...
echo.

REM Build as single-file EXE
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo Your STANDALONE executable is ready (no .NET needed!):
    echo bin\Release\net6.0\win-x64\publish\AMSIBypass.exe
    echo.
    echo File size:
    for %%A in (bin\Release\net6.0\win-x64\publish\AMSIBypass.exe) do echo %%~zA bytes
    echo.
    echo Run with: AMSIBypass.exe
) else (
    echo.
    echo ========================================
    echo BUILD FAILED!
    echo ========================================
    echo.
    echo Make sure .NET 6.0 SDK or later is installed.
    echo Download: https://dotnet.microsoft.com/download
)

echo.
pause
