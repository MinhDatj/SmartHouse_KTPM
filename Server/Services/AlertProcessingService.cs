using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Server.Models; 

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
                DateTime thoiGian = sensorData.GetProperty("ThoiGian").GetDateTime();
                bool hasAlert = false;

                // --- 1. Xử lý Phòng Khách ---
                var khach = sensorData.GetProperty("Khach");
                bool cuaMo = khach.GetProperty("CuaChinhMo").GetBoolean();
                bool tiviBat = khach.GetProperty("TiviBat").GetBoolean();
                bool quatBat = khach.GetProperty("QuatBat").GetBoolean();
                bool denBatKhach = khach.GetProperty("DenBat").GetBoolean();

                if (cuaMo)
                {
                    await TriggerAlert(thoiGian, "Phòng Khách", "Security", "Cửa chính đang mở. Cảnh báo an ninh!", ConsoleColor.Yellow);
                    hasAlert = true;
                }
                else if (tiviBat || quatBat || denBatKhach) 
                {
                    await TriggerAlert(thoiGian, "Phòng Khách", "Energy", "Quên tắt Tivi/Quạt/Đèn khi không có nhà.", ConsoleColor.Cyan);
                    hasAlert = true;
                }

                // --- 2. Xử lý Phòng Bếp ---
                var bep = sensorData.GetProperty("Bep");
                double nhietDoBep = bep.GetProperty("NhietDo").GetDouble();
                bool khoiBep = bep.GetProperty("PhatHienKhoi").GetBoolean();
                bool bepTuBat = bep.GetProperty("BepTuBat").GetBoolean();

                if (nhietDoBep > 50.0 || khoiBep)
                {
                    await TriggerAlert(thoiGian, "Phòng Bếp", "Fire", "Phát hiện nhiệt độ cao hoặc có khói. Nguy cơ cháy nổ!", ConsoleColor.Red);
                    hasAlert = true;
                }
                else if (bepTuBat && !khoiBep && nhietDoBep < 40.0)
                {
                     await TriggerAlert(thoiGian, "Phòng Bếp", "Safety", "Bếp từ đang bật nhưng không sử dụng.", ConsoleColor.DarkYellow);
                     hasAlert = true;
                }

                // --- 3. Xử lý Phòng Ngủ ---
                var ngu = sensorData.GetProperty("Ngu");
                bool dieuHoaNgu = ngu.GetProperty("DieuHoaBat").GetBoolean();
                
                if (dieuHoaNgu && cuaMo) 
                {
                     // Báo động cụ thể như em mong muốn
                     await TriggerAlert(thoiGian, "Phòng Ngủ", "Energy", "Không tắt điều hòa khi đang không sử dụng hay đã đi ra khỏi nhà.", ConsoleColor.Cyan);
                     hasAlert = true;
                }

                // --- 4. Xử lý Phòng Tắm ---
                var tam = sensorData.GetProperty("Tam");
                bool binhNongLanh = tam.GetProperty("BinhNongLanhBat").GetBoolean();
                double nhietDoTam = tam.GetProperty("NhietDo").GetDouble();

                if (binhNongLanh && nhietDoTam > 30.0)
                {
                    await TriggerAlert(thoiGian, "Phòng Tắm", "Energy", "Phòng tắm nóng, hãy tắt bình nóng lạnh để tiết kiệm điện.", ConsoleColor.Cyan);
                    hasAlert = true;
                }

                // --- Trạng thái an toàn toàn cục ---
                if (!hasAlert)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{thoiGian.AddHours(7):HH:mm:ss}] Trạng thái: Mọi phòng hoạt động bình thường.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi phân tích logic phòng: {ex.Message}");
            }
        }

        // --- HÀM GỬI CẢNH BÁO ---
        private async Task TriggerAlert(DateTime time, string roomName, string type, string message, ConsoleColor color)
        {
            // 1. In ra Console 
            Console.ForegroundColor = color;
            Console.WriteLine($"[{time.AddHours(7):HH:mm:ss}] [{roomName}] [{type}] {message}");
            Console.ResetColor();

            // 2. Bắn sang WPF Client cho Bạn 4 
            await _hubContext.Clients.All.SendAsync("ReceiveAlert", new AlertPayload 
            {
                AlertType = type,
                CurrentValue = 0.0, 
                Message = $"[{roomName}] {message}", 
                Timestamp = time.AddHours(7), 
                Status = "WARNING"
            });
        }
    }
}