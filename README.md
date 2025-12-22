# PLC Visualisierung für Siemens S7-1500

Web-basierte Visualisierung für bis zu 100 Antriebe einer Siemens S7-1500 SPS.

## Features

- ✅ Real-time Überwachung von bis zu 100 Antrieben
- ✅ Hand/Auto-Modus für jeden Antrieb
- ✅ Vorwärts/Rückwärts Steuerung
- ✅ Sollwert-Vorgabe
- ✅ Anzeige von Drehzahl, Strom und Fehlern
- ✅ Konfigurierbare Anzeige pro Antrieb
- ✅ SignalR für Real-time Updates
- ✅ LAN-Zugriff von beliebigen Geräten

## Technologie-Stack

- **Backend**: ASP.NET Core (.NET 10) mit Blazor Server
- **PLC-Kommunikation**: S7.Net Plus
- **Real-time**: SignalR
- **Frontend**: Blazor mit interaktiven Komponenten
- **Styling**: Custom CSS

## Voraussetzungen

- .NET 10 SDK
- Siemens S7-1500 SPS
- Netzwerkverbindung zur SPS

## Installation & Start

### 1. Projekt klonen

```bash
git clone https://github.com/IHR-USERNAME/PlcVisualization.git
cd PlcVisualization
```

### 2. PLC-Einstellungen anpassen

Bearbeiten Sie `PlcVisualization/appsettings.json`:

```json
{
  "PlcSettings": {
    "IpAddress": "192.168.0.1",  // IP-Adresse Ihrer SPS
    "Rack": 0,
    "Slot": 1,
    "DataBlock": 100,             // DB-Nummer in der SPS
    "DriveCount": 100,            // Anzahl Antriebe
    "UpdateIntervalMs": 100       // Polling-Intervall
  }
}
```

### 3. Starten

**Option A: Mit der BAT-Datei (empfohlen für LAN-Zugriff)**

Doppelklick auf `start.bat`

Die Anwendung ist dann erreichbar unter:
- Lokal: `http://localhost:5000`
- Im LAN: `http://[IP-DIESER-PC]:5000`

**Option B: Manuell**

```bash
cd PlcVisualization
dotnet run
```

## SPS-Konfiguration (TIA Portal)

### 1. UDT erstellen (UDT_Drive)

```pascal
TYPE "UDT_Drive"
STRUCT
    ModeAuto : Bool;        // Bit 0.0
    Running : Bool;         // Bit 0.1
    Forward : Bool;         // Bit 0.2
    Reverse : Bool;         // Bit 0.3
    Error : Bool;           // Bit 0.4
    HasSetpoint : Bool;     // Bit 0.5
    HasSpeed : Bool;        // Bit 0.6
    HasCurrent : Bool;      // Bit 0.7
    Reserved : Byte;        // Byte 1
    Speed : Int;            // Byte 2-3 (RPM)
    Current : Int;          // Byte 4-5 (0.1 A)
    Setpoint : Int;         // Byte 6-7 (RPM)
    ErrorCode : Int;        // Byte 8-9
END_STRUCT;
// Gesamt: 10 Bytes
```

### 2. Datenbaustein erstellen (DB100)

**WICHTIG**: `S7_Optimized_Access := 'FALSE'` deaktivieren!

```pascal
DATA_BLOCK "DB_Drives"
{ S7_Optimized_Access := 'FALSE' }
VERSION : 0.1
NON_RETAIN
VAR
    Drives : Array[1..100] of "UDT_Drive";
END_VAR
```

### 3. Zugriffsrechte

Stellen Sie sicher, dass die SPS-Verbindung erlaubt ist:
- In TIA Portal: PLC → Properties → Protection → Connection mechanisms
- "Permit access with PUT/GET" aktivieren

## Anpassung der Antriebskonfiguration

Derzeit werden alle Antriebe mit den gleichen Capabilities erstellt. Um individuelle Konfigurationen zu speichern, können Sie:

1. Eine SQLite-Datenbank hinzufügen
2. Eine JSON-Konfigurationsdatei verwenden
3. Die Capabilities aus den SPS-Flags lesen (HasSetpoint, HasSpeed, HasCurrent)

## Troubleshooting

### Verbindung zur SPS funktioniert nicht

- Prüfen Sie die IP-Adresse in `appsettings.json`
- Prüfen Sie ob die SPS erreichbar ist (Ping)
- Prüfen Sie die Firewall-Einstellungen
- Prüfen Sie die SPS-Zugriffsrechte

### Kein Zugriff von anderen PCs im LAN

- Prüfen Sie die Windows Firewall (Port 5000 freigeben)
- Nutzen Sie die `start.bat` Datei
- Prüfen Sie die IP-Adresse des Servers

### Antriebe werden nicht angezeigt

- Prüfen Sie die Browser-Konsole (F12)
- Prüfen Sie die Server-Logs
- Stellen Sie sicher, dass die SPS-Struktur korrekt ist

## Projektstruktur

```
PlcVisualization/
├── PlcVisualization/
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Drives.razor        # Hauptseite
│   │   └── Layout/
│   ├── Models/
│   │   ├── DriveData.cs            # SPS-Datenstruktur
│   │   ├── DriveState.cs           # Frontend DTO
│   │   └── DriveCommand.cs         # Steuerkommandos
│   ├── Services/
│   │   └── PlcService.cs           # PLC-Kommunikation
│   ├── Hubs/
│   │   └── DriveHub.cs             # SignalR Hub
│   ├── wwwroot/
│   │   └── app.css                 # Styling
│   └── Program.cs
├── start.bat                        # LAN-Start-Script
└── README.md
```

## Lizenz

MIT License

## Autor

Erstellt für die Visualisierung von Industrieanlagen mit Siemens S7-1500 SPS.
