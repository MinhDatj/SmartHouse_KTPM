using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Services; // Gọi đường dẫn tới thư mục chứa bộ não cảnh báo của em

namespace Server
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly AlertProcessingService _alertService; // Khai báo Service của em

        // Bổ sung AlertProcessingService vào constructor (Dependency Injection)
        public Worker(ILogger<Worker> logger, AlertProcessingService alertService)
        {
            _logger = logger;
            _alertService = alertService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hệ thống IoT SmartBuilding đang chạy... Chờ gói tin MQTT.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. GIẢ LẬP NHẬN GÓI TIN MQTT (Sau này Tech Lead ghép MQTT thật thì xóa hàm Generate này đi)
                string simulatedJsonPayload = GenerateSimulatedData(); 
                
                try 
                {
                    // Chuyển chuỗi JSON thành đối tượng JsonElement để Service của em có thể đọc
                    using var document = JsonDocument.Parse(simulatedJsonPayload);
                    var sensorData = document.RootElement;

                    // _logger.LogInformation($"[Worker Nhận JSON]: {simulatedJsonPayload}");

                    // 2. KIẾN TRÚC ĐA LUỒNG (CHẠY SONG SONG TUYỆT ĐỐI)
                    
                    // Luồng 1: Của Bạn 2 (Lưu Database)
                    var dbTask = Task.Run(() => 
                    {
                        // Bạn 2 sẽ viết code lưu Entity Framework SQL Server ở đây
                        // _databaseService.SaveToDb(sensorData);
                        Task.Delay(100).Wait(); // Tạm thời giả lập mất 100ms để lưu DB
                    });

                    // Luồng 2: Của Châu (Xử lý cảnh báo & Bắn SignalR)
                    var alertTask = Task.Run(async () => 
                    {
                        await _alertService.ProcessDataAsync(sensorData);
                    });

                    // Lệnh cốt lõi: Yêu cầu CPU chạy cả 2 luồng trên cùng lúc và đợi cả 2 xong mới đi tiếp
                    await Task.WhenAll(dbTask, alertTask);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi xử lý đa luồng: {ex.Message}");
                }

                // Chờ 5 giây rồi lặp lại (Khớp với chu kỳ sinh dữ liệu của Component 1)
                await Task.Delay(5000, stoppingToken);
            }
        }

        // Hàm hỗ trợ: Sinh random dữ liệu JSON để em test hệ thống chớp đỏ ngay trên máy
        private string GenerateSimulatedData()
        {
            var random = new Random();
            
            // Random nhiệt độ từ 20 đến 80 độ C (Để chắc chắn có lúc vượt ngưỡng 60 độ)
            double nhietDo = Math.Round(random.NextDouble() * 60 + 20, 1); 
            
            // 15% xác suất có khói
            bool coKhoi = random.NextDouble() > 0.85; 

            var data = new
            {
                CanHoId = "CH-" + random.Next(101, 105), // Random phòng từ CH-101 đến CH-104
                NhietDo = nhietDo,
                Khoi = coKhoi
            };

            return JsonSerializer.Serialize(data);
        }
    }
}