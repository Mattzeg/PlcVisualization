using System;

namespace PlcVisualization.Models
{
    /// <summary>
    /// Struktur für Antriebsdaten aus der SPS (DB100)
    /// Muss exakt der SPS-Struktur entsprechen (UDT_Drive)
    /// Gesamt: 10 Bytes pro Antrieb
    /// </summary>
    public class DriveData
    {
        // Byte 0: Status-Flags
        public bool ModeAuto { get; set; }      // DB.DBX0.0 - Auto/Hand Modus
        public bool Running { get; set; }       // DB.DBX0.1 - Motor läuft
        public bool Forward { get; set; }       // DB.DBX0.2 - Vorwärtslauf
        public bool Reverse { get; set; }       // DB.DBX0.3 - Rückwärtslauf
        public bool Error { get; set; }         // DB.DBX0.4 - Fehlerzustand
        public bool HasSetpoint { get; set; }   // DB.DBX0.5 - Sollwert verfügbar
        public bool HasSpeed { get; set; }      // DB.DBX0.6 - Drehzahl verfügbar
        public bool HasCurrent { get; set; }    // DB.DBX0.7 - Strom verfügbar

        // Byte 1: Reserviert für zukünftige Erweiterungen
        public byte Reserved { get; set; }      // DB.DBB1

        // Byte 2-3: Drehzahl (Int/Word)
        public short Speed { get; set; }        // DB.DBW2 - Drehzahl in RPM

        // Byte 4-5: Strom (Int/Word)
        public short Current { get; set; }      // DB.DBW4 - Strom in 0.1 A (z.B. 150 = 15.0 A)

        // Byte 6-7: Sollwert (Int/Word)
        public short Setpoint { get; set; }     // DB.DBW6 - Sollwert RPM

        // Byte 8-9: Fehlercode
        public short ErrorCode { get; set; }    // DB.DBW8 - Fehlercode
    }
}
