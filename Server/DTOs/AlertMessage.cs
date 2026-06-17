using System;

namespace SmartBuilding.Server.DTOs
{
    public class AlertMessage
    {
        public string AlertId { get; set; } = string.Empty;
        public int ApartmentId { get; set; }
        public string ApartmentName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}