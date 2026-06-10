using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Server.Services; // Đảm bảo đúng namespace nơi chứa AlertProcessingService của em

namespace Server;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AlertProcessingService _alertService;
    private IMqttClient _mqttClient;

    // Constructor bơm Dependency Injection (DI)
    public Worker(ILogger<Worker> logger, AlertProcessingService alertService)
    {
        _logger = logger;
        _alertService = alertService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== Hệ thống IoT SmartBuilding Server đang khởi động MQTT... ===");

        // 1. Cấu hình MQTT Client (chuẩn bản v4.3.7)
        var mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com", 1883)
            .WithClientId($"SmartHouse_Server_{Guid.NewGuid()}") // Phân biệt với ID của Simulator
            .Build();

        // 2. Định nghĩa hành động KHI NHẬN ĐƯỢC DATA từ cảm biến
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            // Giải mã gói tin thành chuỗi String
            string jsonPayload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            
            try
            {
                // Chuyển chuỗi JSON thành đối tượng JsonElement để truyền đi
                using JsonDocument doc = JsonDocument.Parse(jsonPayload);
                JsonElement sensorData = doc.RootElement.Clone();

                _logger.LogInformation($"[NHẬN DATA MẠNG]: {jsonPayload}");

                // --- KIẾN TRÚC ĐA LUỒNG (CHẠY SONG SONG TUYỆT ĐỐI) ---
                
                // Luồng 1: Của Bạn 2 (Giao cho nhánh lưu Database)
                var dbTask = Task.Run(() => 
                {
                    // _databaseService.SaveToDb(sensorData);
                    Task.Delay(100).Wait(); // Giả lập việc lưu DB mất 100ms
                });

                // Luồng 2: Của Châu (Giao cho Bộ não xử lý cảnh báo & Bắn SignalR)
                var alertTask = Task.Run(async () => 
                {
                    await _alertService.ProcessDataAsync(sensorData);
                });

                // Ép hệ thống xuất phát chạy 2 luồng cùng 1 lúc!
                await Task.WhenAll(dbTask, alertTask);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[LỖI XỬ LÝ DỮ LIỆU]: {ex.Message}");
            }
        };

        // 3. Kết nối và Subscribe (Lắng nghe đúng đường ống)
        try
        {
            await _mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);
            _logger.LogInformation("-> Đã kết nối thành công tới MQTT Broker (HiveMQ)!");

            // Đăng ký lắng nghe đúng cái Topic mà Simulator đang bắn lên
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("smarthouse/sensors"))
                .Build();

            await _mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);
            _logger.LogInformation("-> Đang lắng nghe tại Topic: smarthouse/sensors");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[LỖI KẾT NỐI MQTT]: {ex.Message}");
        }

        // Vòng lặp vô hạn để giữ cho Server (Worker) luôn sống và lắng nghe
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}