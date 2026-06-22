using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace WPF_Shared.Services
{
    // 1. CÁC CLASS DTO ĐỂ HỨNG DỮ LIỆU TỪ SERVER GỬI XUỐNG
    public class SensorData { public string Type { get; set; } public double Value { get; set; } }
    public class TelemetryMessage { public int ApartmentId { get; set; } public List<SensorData> Sensors { get; set; } }
    public class AlertMessage { public int ApartmentId { get; set; } public string ApartmentName { get; set; } public string Message { get; set; } }

    // 2. CLASS DỊCH VỤ SIGNALR
    public class SignalRService
    {
        private readonly HubConnection _connection;

        // Các sự kiện (Event) để ViewModel đăng ký theo dõi
        public event Action<double, double, double> OnRealTimeDataUpdated;
        public event Action<int, string, string> OnAlertTriggered;

        public SignalRService()
        {
            // LƯU Ý: Nhớ đổi "5000" thành đúng cái Port mà Backend ASP.NET Core đang chạy
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/sensorHub")
                .WithAutomaticReconnect() // Tự động kết nối lại nếu Server rớt mạng
                .Build();

            // Lắng nghe kênh "ReceiveTelemetry" (Do Worker.cs trên Server bắn xuống mỗi 5s)
            _connection.On<TelemetryMessage>("ReceiveTelemetry", (data) =>
            {
                if (data?.Sensors != null)
                {
                    double temp = 0, hum = 0, smoke = 0;
                    foreach (var s in data.Sensors)
                    {
                        if (s.Type == "Temperature") temp = s.Value;
                        if (s.Type == "Humidity") hum = s.Value;
                        if (s.Type == "Smoke") smoke = s.Value;
                    }
                    // Bắn sự kiện ra ngoài cho ViewModel bắt lấy
                    OnRealTimeDataUpdated?.Invoke(temp, hum, smoke);
                }
            });

            // Lắng nghe kênh "ReceiveAlert" (Do Bạn 3 - AlertService bắn xuống khi có cháy)
            _connection.On<AlertMessage>("ReceiveAlert", (alert) =>
            {
                // Truyền ID căn hộ và Lời nhắn báo động
                OnAlertTriggered?.Invoke(alert.ApartmentId, alert.ApartmentName, alert.Message);
            });
        }

        // Hàm gọi để bắt đầu kết nối
        public async Task ConnectAsync()
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("Đã kết nối thành công tới Server SignalR!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối SignalR: {ex.Message}");
            }
        }

        public async Task SendSupportRequestToServerAsync(int apartmentId, string message)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                try
                {
                    // Gọi hàm "RelayAlertToManager" trên Backend Hub của Server
                    await _connection.InvokeAsync("RelayAlertToManager", apartmentId, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi gửi cảnh báo: {ex.Message}");
                }
            }
        }
    }
}