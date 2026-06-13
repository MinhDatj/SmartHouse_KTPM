using System;
using System.Text.Json;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client; // Em giữ using này nếu dùng bản v4.3.7 nhé

namespace Simulator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com", 1883)
                .Build();

            Console.WriteLine("Dang ket noi toi MQTT Broker...");
            await mqttClient.ConnectAsync(mqttClientOptions);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">> Da ket noi thanh cong toi MQTT Broker (HiveMQ)! <<\n");
            Console.ResetColor();

            var random = new Random();

            while (true)
            {
                var data = new SensorData
                {
                    ThoiGian = DateTime.UtcNow,
                    
                    Khach = new PhongKhach
                    {
                        CuaChinhMo = random.NextDouble() > 0.8, // 20% xác suất mở cửa
                        TiviBat = random.NextDouble() > 0.5,
                        DieuHoaBat = random.NextDouble() > 0.7,
                        QuatBat = random.NextDouble() > 0.4,
                        DenBat = random.NextDouble() > 0.5,
                        NhietDo = Math.Round(random.NextDouble() * 15 + 20, 1) // 20-35 độ
                    },
                    Bep = new PhongBep
                    {
                        BepTuBat = random.NextDouble() > 0.6,
                        MayHutMuiBat = random.NextDouble() > 0.6,
                        DenBat = random.NextDouble() > 0.5,
                        NhietDo = Math.Round(random.NextDouble() * 30 + 25, 1), // 25-55 độ
                        PhatHienKhoi = random.NextDouble() > 0.9 // 10% có khói
                    },
                    Ngu = new PhongNgu
                    {
                        DieuHoaBat = random.NextDouble() > 0.4,
                        DenBat = random.NextDouble() > 0.6,
                        NhietDo = Math.Round(random.NextDouble() * 10 + 20, 1) // 20-30 độ
                    },
                    Tam = new PhongTam
                    {
                        DenBat = random.NextDouble() > 0.7,
                        DenSuoiBat = random.NextDouble() > 0.8,
                        BinhNongLanhBat = random.NextDouble() > 0.6,
                        NhietDo = Math.Round(random.NextDouble() * 10 + 22, 1) // 22-32 độ
                    }
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonPayload = JsonSerializer.Serialize(data, options);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("smarthouse/sensors")
                    .WithPayload(jsonPayload)
                    .Build();

                await mqttClient.PublishAsync(message);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Da gui trang thai cac phong:\n{jsonPayload}\n---");

                await Task.Delay(5000);
            }
        }
    }
}