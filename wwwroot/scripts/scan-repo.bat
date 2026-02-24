@echo off
setlocal EnableDelayedExpansion

REM ==============================
REM CONFIGURATION
REM ==============================
set GITLEAKS_EXE=C:\Tools\gitleaks_8.30.0_windows_x64\gitleaks.exe
set APP_ROOT=C:\Users\azureuser\Source\Repos\migration_accelerator\migration_accelerator
set SCAN_DIR=%APP_ROOT%\wwwroot\scan-results
set DATA_DIR=%APP_ROOT%\wwwroot\data

if not exist "%SCAN_DIR%" mkdir "%SCAN_DIR%"
if not exist "%DATA_DIR%" mkdir "%DATA_DIR%"

echo.
set /p REPO_NAME=Enter Repo Name: 
set REPO_PATH=%CD%

echo --------------------------------
echo Repo Name : %REPO_NAME%
echo Repo Path : %REPO_PATH%
echo --------------------------------

REM ==============================
REM RUN GITLEAKS
REM ==============================
"%GITLEAKS_EXE%" detect ^
 --source "%REPO_PATH%" ^
 --no-git ^
 --report-format json ^
 --report-path "%SCAN_DIR%\%REPO_NAME%.json" ^
 --exit-code 0

REM ==============================
REM ADD METADATA TO REPORT
REM ==============================
powershell -NoProfile -ExecutionPolicy Bypass ^
 -File "%DATA_DIR%\wrap-gitleaks-report.ps1" ^
 -RepoName "%REPO_NAME%" ^
 -ReportPath "%SCAN_DIR%\%REPO_NAME%.json"

REM ==============================
REM COUNT SECRETS
REM ==============================
for /f %%A in ('powershell -NoProfile -Command "(Get-Content '%SCAN_DIR%\%REPO_NAME%.json' | ConvertFrom-Json).findings.Count"') do set SECRET_COUNT=%%A

REM ==============================
REM CALCULATE REPO SIZE
REM ==============================
for /f %%A in ('powershell -NoProfile -Command ^
  "[math]::Round((Get-ChildItem '%REPO_PATH%' -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 2)"') do set SIZE_MB=%%A

REM ==============================
REM COMPLEXITY LOGIC
REM ==============================
set COMPLEXITY=Easy
if %SECRET_COUNT% GEQ 5 set COMPLEXITY=Medium
if %SECRET_COUNT% GEQ 10 set COMPLEXITY=Hard

REM ==============================
REM UPDATE SUMMARY REPORT
REM ==============================
powershell -NoProfile -ExecutionPolicy Bypass ^
 -File "%DATA_DIR%\update-migration-report.ps1" ^
 -RepoName "%REPO_NAME%" ^
 -RepoSizeMB "%SIZE_MB%" ^
 -SensitiveCount %SECRET_COUNT% ^
 -Complexity "%COMPLEXITY%"

echo.
echo ✅ Scan completed successfully
pause
