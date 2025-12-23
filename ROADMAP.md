# PlcVisualization - Roadmap & Geplante Features

## ✅ Fertiggestellt (v1.2)

- [x] Grundlegende Web-Visualisierung
- [x] S7.Net Kommunikation mit S7-1500
- [x] 100 Antriebe gleichzeitig
- [x] Hand/Auto-Modus pro Antrieb
- [x] Bidirektionale Steuerung (Lesen & Schreiben)
- [x] Real-time Updates über SignalR
- [x] Konfiguration über appsettings.json
- [x] Portable self-contained Version
- [x] LAN-Zugriff von anderen Geräten

## 🚀 In Arbeit (v1.3)

### Option A: Antrieb-Konfiguration (SQLite Datenbank)
- [ ] SQLite Datenbank hinzufügen
- [ ] DriveConfiguration Model erstellen
- [ ] Individuelle Namen pro Antrieb ("Förderer 1" statt "Antrieb 1")
- [ ] Capabilities pro Antrieb konfigurierbar
  - [ ] HasForward
  - [ ] HasReverse
  - [ ] HasSetpoint
  - [ ] HasSpeedDisplay
  - [ ] HasCurrentDisplay
- [ ] Min/Max Werte für Sollwerte
- [ ] Beschreibung/Notizen pro Antrieb
- [ ] Konfigurations-Seite im Frontend
- [ ] Datenbank-Migration und Initialisierung

## 📋 Backlog (Zukünftige Versionen)

### Option B: Historie & Logging
- [ ] Werte-Historie in Datenbank speichern
  - [ ] Speed-Verlauf über Zeit
  - [ ] Laufzeiten pro Antrieb
  - [ ] Schaltzyklen zählen
- [ ] Fehler-Log System
  - [ ] Zeitstempel bei Fehlern
  - [ ] Fehlercode-Historie
  - [ ] Fehlerstatistiken
- [ ] Grafische Darstellung (Charts)
  - [ ] Line-Charts für Verläufe
  - [ ] Zeitbereich auswählbar
- [ ] Export-Funktionen
  - [ ] CSV Export
  - [ ] Excel Export
  - [ ] PDF-Berichte

### Option C: SVG-Anlagenlayout
- [ ] SVG-Editor Integration
  - [ ] Drag & Drop für Antriebe
  - [ ] Positionierung auf Anlagenlayout
- [ ] Mehrere Layouts
  - [ ] Verschiedene Ansichten (Halle 1, 2, etc.)
  - [ ] Layout-Switcher
- [ ] Visualisierung
  - [ ] Antriebe an echten Positionen
  - [ ] Farbcodierung (Running=grün, Error=rot)
  - [ ] Verbindungslinien zwischen Komponenten
- [ ] Zoom & Pan Funktionen

### Option D: Benutzer-Verwaltung
- [ ] ASP.NET Identity Integration
- [ ] Login-System
  - [ ] Registrierung
  - [ ] Passwort-Reset
- [ ] Rollen-System
  - [ ] Admin (volle Rechte)
  - [ ] Operator (Steuerung erlaubt)
  - [ ] Viewer (nur Ansicht)
- [ ] Audit-Log
  - [ ] Wer hat was wann geändert
  - [ ] Änderungshistorie
  - [ ] Log-Ansicht für Admins

### Option E: Alarms & Notifications
- [ ] E-Mail Benachrichtigungen
  - [ ] SMTP Konfiguration
  - [ ] Templates für verschiedene Events
  - [ ] Empfängerliste konfigurierbar
- [ ] SMS Benachrichtigungen (optional)
  - [ ] Twilio oder ähnlicher Dienst
- [ ] Browser-Benachrichtigungen
  - [ ] Web Push Notifications
  - [ ] Browser-Notification API
- [ ] Akustische Signale
  - [ ] Sound bei Fehler
  - [ ] Konfigurierbare Sounds
- [ ] Alarm-Verwaltung
  - [ ] Alarm quittieren
  - [ ] Alarm-Historie
  - [ ] Eskalations-Regeln

## 🔮 Ideen für die Zukunft

### Performance & Skalierung
- [ ] Mehrere SPS-Verbindungen parallel
- [ ] Load Balancing bei vielen Clients
- [ ] Daten-Caching optimieren
- [ ] Websocket Compression

### Mobile App
- [ ] Progressive Web App (PWA)
- [ ] Offline-Fähigkeit
- [ ] Native Mobile App (Xamarin/MAUI)

### Integration
- [ ] OPC UA Server zusätzlich zu S7
- [ ] Modbus TCP Support
- [ ] REST API für externe Systeme
- [ ] MQTT Integration

### Erweiterte Funktionen
- [ ] Rezeptverwaltung
- [ ] Sequenz-Steuerung
- [ ] Automatische Diagnose
- [ ] Predictive Maintenance
- [ ] KPI Dashboard

### UI/UX Verbesserungen
- [ ] Dark Mode
- [ ] Mehrsprachigkeit (i18n)
- [ ] Responsive Design verbessern
- [ ] Touchscreen-Optimierung
- [ ] Shortcuts/Hotkeys

## 📊 Prioritäten

**Kurzfristig (nächste 2 Wochen):**
1. ✅ Option A: Antrieb-Konfiguration (v1.3)

**Mittelfristig (nächster Monat):**
2. Option B: Historie & Logging (v1.4)
3. Option E: Basis-Alarms (v1.5)

**Langfristig:**
4. Option C: SVG-Layout (v2.0)
5. Option D: Benutzer-Verwaltung (v2.1)

## 📝 Notizen

- Alle Features sollen optional sein (über Konfiguration aktivierbar)
- Rückwärtskompatibilität bewahren
- Performance im Fokus behalten (100ms Polling-Intervall)
- Dokumentation für jedes Feature
- Unit Tests für kritische Komponenten

---

**Letzte Aktualisierung:** 2025-12-22
**Aktuelle Version:** v1.2
**Nächste Version:** v1.3 (Antrieb-Konfiguration)
