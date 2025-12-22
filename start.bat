@echo off
echo ========================================
echo PLC Visualisierung Server
echo ========================================
echo.
echo Starte Server auf allen Netzwerkinterfaces...
echo Von anderen Rechnern erreichbar unter:
echo http://[DIESE-PC-IP]:5000
echo.
echo Druecke Strg+C zum Beenden
echo ========================================
echo.

cd /d "%~dp0PlcVisualization"
dotnet run --urls "http://0.0.0.0:5000"

pause
