using System.Collections.Generic;

namespace SmartBuilding.Server.DTOs
{
    public class TelemetryMessage
    {
        public int ApartmentId { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public List<SensorData> Sensors { get; set; } = new List<SensorData>();
    }

    public class SensorData
    {
        public int SensorId { get; set; }
        public string Type { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Room { get; set; } = string.Empty;
    }
}