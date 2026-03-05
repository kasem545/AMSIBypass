@echo off
REM ============================================================================
REM AMSI Bypass - Standalone EXE Builder
REM Creates a SINGLE executable with ALL dependencies embedded (no DLLs needed)
REM ============================================================================

echo.
echo ========================================================================
echo                    AMSI Bypass Standalone Builder
echo ========================================================================
echo.
echo This script creates a SINGLE .exe file that includes:
echo   - All .NET runtime libraries
echo   - All dependencies
echo   - No separate DLL files needed
echo.

REM Check if .NET SDK is installed
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found!
    echo.
    echo Please install .NET 6.0 SDK or later:
    echo https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
if exist bin rmdir /s /q bin 2>nul
if exist obj rmdir /s /q obj 2>nul
echo       Done.
echo.

echo [2/5] Restoring packages...
dotnet restore >nul 2>&1
echo       Done.
echo.

echo [3/5] Building Release configuration...
dotnet build -c Release >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed! Check for compilation errors.
    echo.
    dotnet build -c Release
    pause
    exit /b 1
)
echo       Done.
echo.

echo [4/5] Publishing as standalone single-file executable...
echo       Target: Windows x64
echo       Mode: Self-contained (includes .NET runtime)
echo       Output: Single EXE file
echo.

dotnet publish -c Release -r win-x64 ^
    /p:PublishSingleFile=true ^
    /p:SelfContained=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true ^
    /p:PublishTrimmed=false ^
    /p:PublishReadyToRun=true ^
    --self-contained true

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)

echo.
echo [5/5] Verifying output...

set EXE_PATH=bin\Release\net6.0\win-x64\publish\AMSIBypass.exe

if not exist "%EXE_PATH%" (
    echo [ERROR] AMSIBypass.exe was not created!
    echo.
    echo Expected location: %EXE_PATH%
    echo.
    pause
    exit /b 1
)

REM Check file size (should be ~60-70 MB for self-contained)
for %%A in ("%EXE_PATH%") do set FILE_SIZE=%%~zA

if %FILE_SIZE% LSS 10000000 (
    echo [WARNING] File size is suspiciously small: %FILE_SIZE% bytes
    echo A proper self-contained build should be 60-70 MB.
    echo You may have built a framework-dependent executable by mistake.
    echo.
) else (
    echo       Done. File size: %FILE_SIZE% bytes
)

echo.
echo ========================================================================
echo                         BUILD SUCCESSFUL!
echo ========================================================================
echo.
echo Standalone executable created:
echo   %EXE_PATH%
echo.
echo File size: %FILE_SIZE% bytes (includes .NET runtime)
echo.
echo This EXE can run on ANY Windows 7+ (x64) machine WITHOUT .NET installed!
echo.
echo ========================================================================
echo.
echo Usage examples:
echo   AMSIBypass.exe              Generate single payload
echo   AMSIBypass.exe -b           Test until bypass found
echo   AMSIBypass.exe -b 50        Test max 50 attempts
echo   AMSIBypass.exe -e           Generate encoded command
echo   AMSIBypass.exe -f out.ps1   Save to file
echo   AMSIBypass.exe -m 5         Generate 5 payloads
echo   AMSIBypass.exe -h           Show help
echo.
echo ========================================================================
echo.

REM Check if any DLL files exist in publish directory (there shouldn't be any)
set DLL_COUNT=0
for %%F in (bin\Release\net6.0\win-x64\publish\*.dll) do set /a DLL_COUNT+=1

if %DLL_COUNT% GTR 0 (
    echo [WARNING] Found %DLL_COUNT% DLL file(s) in publish directory.
    echo This means the build is NOT a true single-file executable.
    echo.
    echo Try rebuilding with:
    echo   dotnet clean
    echo   dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true
    echo.
)

pause
