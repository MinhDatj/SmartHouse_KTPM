using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MQTTnet;
using MQTTnet.Client;

namespace SmartBuilding.Simulator
{
    internal class Program
    {
        private static IMqttClient _mqttClient;
        private static System.Timers.Timer _telemetryTimer;
        private static bool _isEmergency = false; // Cờ hiệu giả lập sự cố

        // Đã sửa thành kiểu số nguyên (int) để khớp với bảng CanHo trong Database
        private static readonly int _idCanHo = 1;

        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== SMART BUILDING SYSTEM - SENSOR SIMULATOR ===");

            // 1. Khởi tạo MQTT Client
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com", 1883) // Broker công cộng
                .WithClientId($"Simulator_Client_{Guid.NewGuid()}")
                .Build();

            // 2. Kết nối tới Broker
            try
            {
                Console.WriteLine("[MQTT] Đang kết nối tới Broker...");
                await _mqttClient.ConnectAsync(mqttOptions, CancellationToken.None);
                Console.WriteLine("[MQTT] Kết nối thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI] Không thể kết nối: {ex.Message}");
                return;
            }

            // 3. Khởi tạo bộ đếm thời gian (5 giây 1 lần)
            _telemetryTimer = new System.Timers.Timer(5000);
            _telemetryTimer.Elapsed += OnTelemetryTimerElapsed;
            _telemetryTimer.AutoReset = true;
            _telemetryTimer.Enabled = true;

            Console.WriteLine("[HỆ THỐNG] Đang chạy vòng lặp gửi dữ liệu định kỳ (5s/lần)...");
            Console.WriteLine("[HỆ THỐNG] Bấm phím 'A' trên bàn phím để BẬT/TẮT giả lập SỰ CỐ HOẢ HOẠN.");
            Console.WriteLine("---------------------------------------------------------");

            // 4. Lắng nghe phím bấm
            ListenForEmergencyKey();
        }

        // Hàm xử lý việc sinh dữ liệu và đẩy lên mạng mỗi 5 giây
        private static async void OnTelemetryTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_mqttClient.IsConnected) return;

            try
            {
                // Điều chỉnh khoảng giá trị nếu đang có sự cố
                double tempMin = _isEmergency ? 65.0 : 24.0;
                double tempMax = _isEmergency ? 85.0 : 35.0;
                double smokeMin = _isEmergency ? 85.0 : 5.0;
                double smokeMax = _isEmergency ? 120.0 : 20.0;

                // Random dữ liệu
                double tempLivingRoom = _random.NextDouble() * (tempMax - tempMin) + tempMin;
                double humidityLivingRoom = _random.Next(50, 81);
                double tempBedroom = _random.NextDouble() * (35 - 24) + 24;
                double tempKitchen = _random.NextDouble() * (tempMax - tempMin) + tempMin;
                double smokeKitchen = _random.Next((int)smokeMin, (int)smokeMax + 1);

                // Đóng gói JSON
                var telemetryData = new
                {
                    ApartmentId = _idCanHo, // Gửi đi số 1
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Sensors = new List<object>
                    {
                        new { Room = "Khach", Type = "Temperature", Value = Math.Round(tempLivingRoom, 1), Unit = "°C" },
                        new { Room = "Khach", Type = "Humidity", Value = Math.Round(humidityLivingRoom, 1), Unit = "%" },
                        new { Room = "Ngu", Type = "Temperature", Value = Math.Round(tempBedroom, 1), Unit = "°C" },
                        new { Room = "Bep", Type = "Temperature", Value = Math.Round(tempKitchen, 1), Unit = "°C" },
                        new { Room = "Bep", Type = "Smoke", Value = Math.Round(smokeKitchen, 1), Unit = "ppm" }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(telemetryData);
                string topic = $"smartbuilding/apartments/{_idCanHo}/telemetry";

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(jsonPayload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                    .Build();

                await _mqttClient.PublishAsync(message, CancellationToken.None);

                if (_isEmergency)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ALARM SENT] -> {topic}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [DATA SENT] -> {topic}");
                }
                Console.WriteLine($"Payload: {jsonPayload}\n");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI] {ex.Message}");
            }
        }

        // Hàm bẫy phím A để giả lập cháy
        private static void ListenForEmergencyKey()
        {
            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.A)
                {
                    _isEmergency = !_isEmergency;
                    Console.BackgroundColor = _isEmergency ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\n>>> ĐÃ CHUYỂN TRẠNG THÁI SỰ CỐ: {(_isEmergency ? "BẬT (Cháy to)" : "TẮT (Bình thường)")} <<<\n");
                    Console.ResetColor();
                }
                else
                {
                }
            }
        }
    }
}