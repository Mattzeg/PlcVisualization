using System;

namespace PlcVisualization.Models
{
    /// <summary>
    /// Protokolliert alle Ereignisse und Änderungen an Antrieben
    /// </summary>
    public class DriveLog
    {
        public int Id { get; set; }
        public int DriveId { get; set; }
        public string DriveName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Art des Ereignisses: Command, StateChange, Error
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Beschreibung des Ereignisses
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Alter Wert (bei Änderungen)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// Neuer Wert (bei Änderungen)
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Benutzer der die Änderung ausgelöst hat (optional für User Management)
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Fehlercode bei Error-Events
        /// </summary>
        public int? ErrorCode { get; set; }

        /// <summary>
        /// Zusätzliche Details im JSON-Format
        /// </summary>
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// Event-Typen für Logging
    /// </summary>
    public static class DriveEventType
    {
        public const string Command = "Command";
        public const string StateChange = "StateChange";
        public const string Error = "Error";
        public const string ErrorCleared = "ErrorCleared";
        public const string ConfigChanged = "ConfigChanged";
    }
}
