using System.ComponentModel.DataAnnotations;

namespace PlcVisualization.Models
{
    /// <summary>
    /// Konfiguration für einen einzelnen Antrieb (gespeichert in SQLite)
    /// </summary>
    public class DriveConfiguration
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Individueller Name (z.B. "Förderer 1", "Mixer Halle 2")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Beschreibung / Notizen
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gruppe/Bereich (z.B. "Halle 1", "Produktionslinie A")
        /// </summary>
        [MaxLength(50)]
        public string? Group { get; set; }

        // === Capabilities ===

        /// <summary>
        /// Antrieb hat Vorwärtslauf
        /// </summary>
        public bool HasForward { get; set; } = true;

        /// <summary>
        /// Antrieb hat Rückwärtslauf
        /// </summary>
        public bool HasReverse { get; set; } = true;

        /// <summary>
        /// Antrieb hat Sollwert-Vorgabe
        /// </summary>
        public bool HasSetpoint { get; set; } = true;

        /// <summary>
        /// Drehzahl-Anzeige verfügbar
        /// </summary>
        public bool HasSpeedDisplay { get; set; } = true;

        /// <summary>
        /// Strom-Anzeige verfügbar
        /// </summary>
        public bool HasCurrentDisplay { get; set; } = true;

        /// <summary>
        /// Fehler-Anzeige verfügbar
        /// </summary>
        public bool HasErrorDisplay { get; set; } = true;

        // === Limits ===

        /// <summary>
        /// Minimaler Sollwert (RPM)
        /// </summary>
        public int MinSetpoint { get; set; } = 0;

        /// <summary>
        /// Maximaler Sollwert (RPM)
        /// </summary>
        public int MaxSetpoint { get; set; } = 3000;

        /// <summary>
        /// Standard Sollwert (RPM)
        /// </summary>
        public int DefaultSetpoint { get; set; } = 1500;

        // === UI ===

        /// <summary>
        /// Position X im SVG-Layout (für zukünftige Verwendung)
        /// </summary>
        public int? LayoutX { get; set; }

        /// <summary>
        /// Position Y im SVG-Layout (für zukünftige Verwendung)
        /// </summary>
        public int? LayoutY { get; set; }

        /// <summary>
        /// Farbe für die Karte (Hex z.B. "#FF5733")
        /// </summary>
        [MaxLength(7)]
        public string? Color { get; set; }

        /// <summary>
        /// Ist dieser Antrieb aktiv/sichtbar?
        /// </summary>
        public bool IsActive { get; set; } = true;

        // === Timestamps ===

        /// <summary>
        /// Wann wurde die Konfiguration erstellt
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Wann wurde die Konfiguration zuletzt geändert
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
