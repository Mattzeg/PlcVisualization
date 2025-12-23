using System;

namespace PlcVisualization.Models
{
    /// <summary>
    /// Position und Darstellung eines Antriebs in einem Layout
    /// </summary>
    public class DriveLayoutPosition
    {
        public int Id { get; set; }

        /// <summary>
        /// Referenz zum Layout
        /// </summary>
        public int LayoutId { get; set; }
        public LayoutConfiguration Layout { get; set; } = null!;

        /// <summary>
        /// Referenz zum Antrieb
        /// </summary>
        public int DriveId { get; set; }

        /// <summary>
        /// X-Position im Layout (in Pixel)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-Position im Layout (in Pixel)
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Breite des Antrieb-Symbols
        /// </summary>
        public double Width { get; set; } = 100;

        /// <summary>
        /// Höhe des Antrieb-Symbols
        /// </summary>
        public double Height { get; set; } = 80;

        /// <summary>
        /// Rotation in Grad (0-360)
        /// </summary>
        public double Rotation { get; set; } = 0;

        /// <summary>
        /// Darstellungstyp (rectangle, circle, motor, custom)
        /// </summary>
        public string ShapeType { get; set; } = "rectangle";

        /// <summary>
        /// Benutzerdefinierte Farbe (überschreibt Standard-Farbcodierung)
        /// </summary>
        public string? CustomColor { get; set; }

        /// <summary>
        /// Label-Text anzeigen?
        /// </summary>
        public bool ShowLabel { get; set; } = true;

        /// <summary>
        /// Label-Position relativ zum Symbol (top, bottom, left, right)
        /// </summary>
        public string LabelPosition { get; set; } = "bottom";

        /// <summary>
        /// Ist der Antrieb in diesem Layout sichtbar?
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Z-Index (Schichtung)
        /// </summary>
        public int ZIndex { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Verfügbare Shape-Typen
    /// </summary>
    public static class DriveShapeType
    {
        public const string Rectangle = "rectangle";
        public const string Circle = "circle";
        public const string Motor = "motor";
        public const string Pump = "pump";
        public const string Conveyor = "conveyor";
        public const string Custom = "custom";
    }

    /// <summary>
    /// Label-Positionen
    /// </summary>
    public static class LabelPositionType
    {
        public const string Top = "top";
        public const string Bottom = "bottom";
        public const string Left = "left";
        public const string Right = "right";
        public const string Center = "center";
    }
}
