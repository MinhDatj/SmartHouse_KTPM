using System;

namespace SmartBuilding.Server.DTOs
{
    public class AlertMessage
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public int ApartmentId { get; set; }
        public string ApartmentName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int SensorId { get; set; }
        public string SensorType { get; set; } = string.Empty;
        public double AlertValue { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public string WarningLevel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}