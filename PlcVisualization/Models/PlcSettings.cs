namespace PlcVisualization.Models
{
    /// <summary>
    /// Konfiguration für die PLC-Verbindung aus appsettings.json
    /// </summary>
    public class PlcSettings
    {
        public string IpAddress { get; set; } = "192.168.0.1";
        public int Rack { get; set; } = 0;
        public int Slot { get; set; } = 1;
        public int DataBlock { get; set; } = 100;
        public int DriveCount { get; set; } = 100;
        public int UpdateIntervalMs { get; set; } = 100;
    }
}
