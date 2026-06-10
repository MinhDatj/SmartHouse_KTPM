using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR; // Cần thiết cho SignalR
using Server.Hubs;                  // Namespace chứa ApartmentHub
using Server.Models;                // Namespace chứa AlertPayload

namespace Server.Services
{
    public class AlertProcessingService
    {
        private readonly ILogger<AlertProcessingService> _logger;
        private readonly IHubContext<ApartmentHub> _hubContext;

        public AlertProcessingService(ILogger<AlertProcessingService> logger, IHubContext<ApartmentHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task ProcessDataAsync(JsonElement sensorData)
        {
            try
            {
                // 1. Bóc tách dữ liệu từ JSON
                string canHoId = sensorData.GetProperty("CanHoId").GetString() ?? "Unknown";
                double nhietDo = sensorData.GetProperty("NhietDo").GetDouble();
                bool khoi = sensorData.GetProperty("Khoi").GetBoolean();
                bool cheDoVangNha = sensorData.GetProperty("CheDoVangNha").GetBoolean();
                bool cuaMo = sensorData.GetProperty("CuaMo").GetBoolean();
                double buiMin = sensorData.GetProperty("BuiMinPM25").GetDouble();
                bool sanUotBan = sensorData.GetProperty("SanNhaUotBan").GetBoolean();
                DateTime thoiGian = sensorData.GetProperty("ThoiGian").GetDateTime();

                bool hasAlert = false;

                // 2. Kiểm tra logic và gửi cảnh báo
                if (nhietDo > 60.0 || khoi)
                {
                    await TriggerAlert(thoiGian, "Fire", "CẢNH BÁO CHÁY", ConsoleColor.Red);
                    hasAlert = true;
                }

                if (cheDoVangNha && cuaMo)
                {
                    await TriggerAlert(thoiGian, "Security", "CẢNH BÁO ĐỘT NHẬP", ConsoleColor.Red);
                    hasAlert = true;
                }

                if (buiMin > 50.0)
                {
                    await TriggerAlert(thoiGian, "AirQuality", "Bụi mịn cao", ConsoleColor.Yellow);
                    hasAlert = true;
                }
                
                if (sanUotBan)
                {
                    await TriggerAlert(thoiGian, "Cleaning", "Sàn bẩn, Robot hoạt động", ConsoleColor.Cyan);
                    hasAlert = true;
                }

                if (!hasAlert)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{thoiGian.AddHours(7):HH:mm:ss}] CH-101: An toàn.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xử lý logic: {ex.Message}");
            }
        }

        // Hàm hỗ trợ: In ra màn hình VÀ gửi sang WPF
        // Sửa hàm TriggerAlert trong AlertProcessingService.cs như sau:
        private async Task TriggerAlert(DateTime time, string type, string message, ConsoleColor color)
        {
            // 1. In ra màn hình Terminal
            Console.ForegroundColor = color;
            Console.WriteLine($"[{time.AddHours(7):HH:mm:ss}] [{type}] {message}");
            Console.ResetColor();

            // 2. Gửi sang WPF - Đã điền đầy đủ các thuộc tính required
            await _hubContext.Clients.All.SendAsync("ReceiveAlert", new AlertPayload 
            {
                ApartmentId = "CH-101",          // Điền ID căn hộ
                AlertType = type,                // Loại cảnh báo
                CurrentValue = 0.0,              // Giá trị cảm biến (nếu không có thì để 0.0)
                Message = message,               // Tin nhắn
                Timestamp = time.AddHours(7),    // Thời gian
                Status = "DANGER"                // Trạng thái
            });
        }
    }
}