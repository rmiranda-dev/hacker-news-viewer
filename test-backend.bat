@echo off
REM Backend Test Runner Script for Windows
REM This script runs all backend tests with various configurations

echo 🚀 Running Backend Tests for Hacker News Viewer
echo =================================================

set BACKEND_DIR=backend\Nextech.Hn.Api.Tests
set COVERAGE_DIR=.\TestResults

REM Ensure we're in the right directory
if not exist "%BACKEND_DIR%" (
    echo ❌ Error: Backend test directory not found. Please run from project root.
    exit /b 1
)

echo.
echo 📦 Restoring packages...
dotnet restore %BACKEND_DIR%
if errorlevel 1 (
    echo ❌ Package restore failed
    exit /b 1
)

echo.
echo 🏗️  Building test project...
dotnet build %BACKEND_DIR% --no-restore
if errorlevel 1 (
    echo ❌ Build failed
    exit /b 1
)

echo.
echo 🧪 Running all tests...
echo ------------------------
dotnet test %BACKEND_DIR% --no-build --verbosity normal
if errorlevel 1 (
    echo ❌ Tests failed
    exit /b 1
)

echo.
echo 📊 Running tests with code coverage...
echo --------------------------------------
REM Create coverage directory
if not exist "%COVERAGE_DIR%" mkdir "%COVERAGE_DIR%"

REM Run tests with coverage collection
dotnet test %BACKEND_DIR% ^
    --no-build ^
    --verbosity normal ^
    /p:CollectCoverage=true ^
    /p:CoverletOutput=./%COVERAGE_DIR%/coverage ^
    /p:CoverletOutputFormat=cobertura

if errorlevel 1 (
    echo ❌ Coverage collection failed
    exit /b 1
)

echo.
echo 📋 Test Results Summary:
echo ========================
echo ✅ All tests completed
echo 📊 Coverage report generated at: %COVERAGE_DIR%\coverage.cobertura.xml
echo.
echo 🎉 Backend testing complete!

REM Optional: Check if reportgenerator is available
where reportgenerator >nul 2>&1
if %errorlevel% == 0 (
    echo.
    echo 📈 Generating HTML coverage report...
    reportgenerator ^
        -reports:"%COVERAGE_DIR%\coverage.cobertura.xml" ^
        -targetdir:"%COVERAGE_DIR%\html" ^
        -reporttypes:Html
    echo 📊 HTML coverage report: %COVERAGE_DIR%\html\index.html
)
