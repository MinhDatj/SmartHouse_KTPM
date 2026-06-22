using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartBuilding.Server.DTOs; // Thư mục chứa DTO
using SmartBuilding.Server.Hubs; // Thư mục chứa SensorHub gốc của Bạn 2
using System;
using System.Threading.Tasks;

namespace SmartBuilding.Server.Services
{
    public class AlertService
    {
        private readonly ILogger<AlertService> _logger;
        private readonly IHubContext<SensorHub> _hubContext;

        public AlertService(ILogger<AlertService> logger, IHubContext<SensorHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task CheckAndFireAlertsAsync(TelemetryMessage telemetryData)
        {
            bool hasIssue = false;

            // Quét qua toàn bộ mảng cảm biến nhận được
            foreach (var sensor in telemetryData.Sensors)
            {
                // LUẬT 1: Cảnh báo cháy (Nhiệt độ > 60°C)
                if (sensor.Type == "Temperature" && sensor.Value > 60.0)
                {
                    hasIssue = true;
                    await PushToWpfClient(telemetryData.ApartmentId, sensor, telemetryData.Timestamp, "DANGER", "Phat hien nhiet do tang cao bat thuong. Nguy co hoa hoan!");
                }

                // LUẬT 2: Cảnh báo khói (> 80 ppm)
                if (sensor.Type == "Smoke" && sensor.Value > 80.0)
                {
                    hasIssue = true;
                    await PushToWpfClient(telemetryData.ApartmentId, sensor, telemetryData.Timestamp, "DANGER", "Canh bao khoi day dac!");
                }
            }

            if (!hasIssue)
            {
                _logger.LogInformation($"[Can ho {telemetryData.ApartmentId}] Cac chi so o muc an toan.");
            }
        }

        // Hàm nội bộ để thực hiện việc bắn dữ liệu Real-time
        private async Task PushToWpfClient(int aptId, SensorData sensor, string time, string level, string warningMsg)
        {
            var alert = new AlertMessage
            {
                ApartmentId = aptId,
                SensorId = sensor.SensorId,
                SensorType = sensor.Type,
                RoomName = sensor.Room,
                AlertValue = sensor.Value,
                Timestamp = time,
                WarningLevel = level,
                Message = warningMsg
            };

            // Ghi log đỏ ra màn hình Server để dễ Debug
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ALERT] {alert.Message} (Chi so: {alert.AlertValue})");
            Console.ResetColor();

            // Bắn tín hiệu SignalR tới Nhóm của Căn hộ đó (Khớp với thiết kế Hub của Bạn 2)
            await _hubContext.Clients.Group($"Apartment_{aptId}").SendAsync("ReceiveAlert", alert);
        }
    }
}