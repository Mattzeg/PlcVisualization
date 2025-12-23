using System;
using System.Collections.Generic;

namespace PlcVisualization.Models
{
    /// <summary>
    /// Konfiguration für ein Anlagenlayout (z.B. Halle 1, Halle 2, Übersicht)
    /// </summary>
    public class LayoutConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>
        /// SVG-Hintergrundbild (Base64 kodiert oder Pfad)
        /// </summary>
        public string? BackgroundSvg { get; set; }

        /// <summary>
        /// Breite des Canvas in Pixel
        /// </summary>
        public int Width { get; set; } = 1920;

        /// <summary>
        /// Höhe des Canvas in Pixel
        /// </summary>
        public int Height { get; set; } = 1080;

        /// <summary>
        /// Grid-Größe für Snap-to-Grid (0 = kein Grid)
        /// </summary>
        public int GridSize { get; set; } = 20;

        /// <summary>
        /// Ist dieses Layout das Standard-Layout?
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Layout aktiv/inaktiv
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sortierreihenfolge
        /// </summary>
        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation zu allen Antriebspositionen in diesem Layout
        /// </summary>
        public List<DriveLayoutPosition> DrivePositions { get; set; } = new();
    }
}
