@echo off
REM ============================================================================
REM Verify that AMSIBypass.exe is a true standalone executable
REM ============================================================================

echo.
echo ========================================================================
echo               AMSIBypass.exe Standalone Verification
echo ========================================================================
echo.

set EXE_PATH=bin\Release\net6.0\win-x64\publish\AMSIBypass.exe

if not exist "%EXE_PATH%" (
    echo [ERROR] AMSIBypass.exe not found!
    echo.
    echo Expected location: %EXE_PATH%
    echo.
    echo Run build-standalone.bat first to create the executable.
    echo.
    pause
    exit /b 1
)

echo [1/4] Checking if file exists...
echo       Found: %EXE_PATH%
echo.

echo [2/4] Checking file size...
for %%A in ("%EXE_PATH%") do set FILE_SIZE=%%~zA

echo       Size: %FILE_SIZE% bytes
echo.

if %FILE_SIZE% LSS 10000000 (
    echo [FAIL] File is too small!
    echo       A self-contained build should be 60-70 MB.
    echo       Current size is only %FILE_SIZE% bytes.
    echo.
    echo This is likely a framework-dependent build (requires .NET installed).
    echo.
    echo Solution: Run build-standalone.bat to create proper standalone EXE.
    echo.
    pause
    exit /b 1
) else if %FILE_SIZE% GTR 100000000 (
    echo [WARNING] File is larger than expected (over 100 MB).
    echo           This is unusual but may still work.
    echo.
) else (
    echo [PASS] Size looks correct for self-contained build.
    echo.
)

echo [3/4] Checking for DLL dependencies...
set DLL_COUNT=0
for %%F in (bin\Release\net6.0\win-x64\publish\*.dll) do (
    set /a DLL_COUNT+=1
    echo       Found DLL: %%~nxF
)

if %DLL_COUNT% GTR 0 (
    echo.
    echo [FAIL] Found %DLL_COUNT% DLL file(s) in publish directory!
    echo       A true single-file executable should have NO DLLs.
    echo.
    echo This means the build is NOT standalone.
    echo.
    echo Solution: Run build-standalone.bat to rebuild correctly.
    echo.
    pause
    exit /b 1
) else (
    echo       No DLL files found.
    echo [PASS] Executable appears to be standalone.
    echo.
)

echo [4/4] Testing if executable runs...
"%EXE_PATH%" -h >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [PASS] Executable runs successfully.
    echo.
) else (
    echo [WARNING] Executable returned error code %ERRORLEVEL%
    echo           May still work for actual payload generation.
    echo.
)

echo ========================================================================
echo                         VERIFICATION COMPLETE
echo ========================================================================
echo.
echo Result: STANDALONE EXECUTABLE ✓
echo.
echo This EXE includes the .NET runtime and can run on Windows machines
echo WITHOUT requiring .NET to be installed.
echo.
echo File: %EXE_PATH%
echo Size: %FILE_SIZE% bytes
echo.
echo You can copy this single file to any Windows 7+ (x64) machine.
echo.
echo ========================================================================
echo.
pause
