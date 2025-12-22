using System;

namespace PlcVisualization.Models
{
    /// <summary>
    /// DTO für das Frontend - enthält Antriebsstatus und Konfiguration
    /// </summary>
    public class DriveState
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Status vom PLC
        public bool ModeAuto { get; set; }
        public bool Running { get; set; }
        public bool Forward { get; set; }
        public bool Reverse { get; set; }
        public bool Error { get; set; }
        public short Speed { get; set; }
        public short Current { get; set; }
        public short Setpoint { get; set; }
        public short ErrorCode { get; set; }
        public DateTime LastUpdate { get; set; }

        // Konfiguration (welche Features hat dieser Antrieb)
        public DriveCapabilities Capabilities { get; set; } = new DriveCapabilities();
    }

    /// <summary>
    /// Definiert welche Steuerungs- und Anzeigeoptionen ein Antrieb hat
    /// </summary>
    public class DriveCapabilities
    {
        public bool HasForward { get; set; } = true;
        public bool HasReverse { get; set; } = true;
        public bool HasSetpoint { get; set; } = true;
        public bool HasSpeedDisplay { get; set; } = true;
        public bool HasCurrentDisplay { get; set; } = true;
        public bool HasErrorDisplay { get; set; } = true;
    }
}
