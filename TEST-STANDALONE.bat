@echo off
REM Test script to verify the EXE is truly standalone

echo ========================================
echo Testing Standalone EXE
echo ========================================
echo.

set EXE_PATH=bin\Release\net6.0\win-x64\publish\AMSIBypass.exe

if not exist "%EXE_PATH%" (
    echo ERROR: Executable not found!
    echo Expected location: %EXE_PATH%
    echo.
    echo Please build first using: build-simple.bat
    pause
    exit /b 1
)

echo Executable found: %EXE_PATH%
echo.

REM Check file size
for %%A in ("%EXE_PATH%") do set SIZE=%%~zA
set /a SIZE_MB=%SIZE%/1048576

echo File size: %SIZE% bytes (%SIZE_MB% MB)
echo.

if %SIZE_MB% LSS 50 (
    echo WARNING: File size is too small!
    echo Expected: 60-80 MB for standalone EXE
    echo Current: %SIZE_MB% MB
    echo.
    echo This is probably NOT a self-contained build.
    echo The EXE will require .NET to be installed on the target machine.
    echo.
    echo Rebuild using: build-simple.bat
    pause
    exit /b 1
)

echo SUCCESS: File size looks good!
echo This appears to be a self-contained executable.
echo.
echo Testing execution...
echo.

REM Try to run with help flag
"%EXE_PATH%" -h

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo TEST PASSED!
    echo ========================================
    echo.
    echo Your EXE is ready to distribute!
    echo It should work on any Windows machine without .NET installed.
    echo.
    echo Location: %EXE_PATH%
) else (
    echo.
    echo ========================================
    echo TEST FAILED!
    echo ========================================
    echo.
    echo The executable failed to run.
    echo Check the error messages above.
)

echo.
pause
