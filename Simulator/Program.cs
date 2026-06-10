using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace Simulator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== KHOI DONG HE THONG GIA LAP SENSOR ===");

        // 1. Cấu hình kết nối tới Broker công cộng HiveMQ
        var mqttFactory = new MqttFactory();
        using var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com", 1883)
            .WithClientId($"SmartHouse_Simulator_{Guid.NewGuid()}") // Tạo ID ngẫu nhiên để không bị trùng
            .Build();

        try
        {
            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-> Da ket noi thanh cong toi MQTT Broker (HiveMQ)!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOI KET NOI]: {ex.Message}");
            return;
        }

        // 2. Vòng lặp sinh dữ liệu mỗi 5 giây
        var random = new Random();

        while (true)
        {
            // TẠO DỮ LIỆU RANDOM THEO KỊCH BẢN
            var data = new SensorData
            {
                CanHoId = "CH-101",
                
                // Kịch bản cháy nổ: Nhiệt độ 20-80 độ, 10% xác suất có khói
                NhietDo = Math.Round(random.NextDouble() * 60 + 20, 1), 
                Khoi = random.NextDouble() > 0.90, 

                // Kịch bản an ninh: 30% xác suất đang vắng nhà, 15% xác suất cửa đang mở
                CheDoVangNha = random.NextDouble() > 0.70, 
                CuaMo = random.NextDouble() > 0.85, 

                // Kịch bản môi trường: Bụi mịn từ 10 (sạch) đến 100 (ô nhiễm nặng)
                BuiMinPM25 = Math.Round(random.NextDouble() * 90 + 10, 1), 

                // Kịch bản dọn dẹp: 20% xác suất sàn nhà bị bẩn/ướt
                SanNhaUotBan = random.NextDouble() > 0.80, 
                
                // --- ĐÂY LÀ DÒNG EM CẦN THÊM VÀO ---
                ThoiGian = DateTime.UtcNow
            };

            // Đóng gói đối tượng thành chuỗi JSON
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonPayload = JsonSerializer.Serialize(data, options);

            // Bắn lên Topic của hệ thống
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("smarthouse/sensors") // Lưu ý tên Topic này, Server sẽ lắng nghe ở đây
                .WithPayload(jsonPayload)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            // In ra màn hình console để em dễ quan sát
            Console.WriteLine($"[Gui luc {DateTime.Now:HH:mm:ss}]: {jsonPayload}");

            // Đợi 5 giây rồi lặp lại
            await Task.Delay(5000);
        }
    }
}