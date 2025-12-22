namespace PlcVisualization.Models
{
    /// <summary>
    /// Kommando vom Frontend zur SPS
    /// </summary>
    public class DriveCommand
    {
        public int DriveId { get; set; }

        // Steuerkommandos (null = keine Änderung)
        public bool? Start { get; set; }
        public bool? Stop { get; set; }
        public bool? Forward { get; set; }
        public bool? Reverse { get; set; }
        public short? Setpoint { get; set; }
        public bool? ModeAuto { get; set; }
    }
}
