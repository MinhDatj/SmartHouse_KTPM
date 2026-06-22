using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using SmartBuilding.Server.DTOs;
using SmartBuilding.Server.Hubs;
using SmartBuilding.Server.Services;

namespace SmartBuilding.Server
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<SensorHub> _hubContext;
        private IMqttClient? _mqttClient;
        private readonly SqlProvider2 _dbProvider;
        private readonly AlertService _alertService;

        public Worker(ILogger<Worker> logger, IHubContext<SensorHub> hubContext, AlertService alertService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _dbProvider = new SqlProvider2();
            _alertService = alertService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com", 1883)
                .WithCleanSession()
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

            try
            {
                await _mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);
                _logger.LogInformation("Da ket noi thanh cong toi MQTT Broker.");

                var topic = "smartbuilding/apartments/+/telemetry";
                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(topic))
                    .Build();

                await _mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi ket noi MQTT.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void WriteTerminal(string payload)
        {
            // 1. In vạch ngăn cách và Tiêu đề có màu Xanh Cyan
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[====== NHAN DU LIEU CAN HO LUC {DateTime.Now:HH:mm:ss} ======]");
            Console.ResetColor();

            try
            {
                // 2. Dàn trang JSON cho đẹp mắt (Pretty Print)
                var parsedJson = JsonDocument.Parse(payload);
                var prettyJson = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { WriteIndented = true });

                // In JSON ra màn hình
                Console.WriteLine(prettyJson);
            }
            catch
            {
                // Nếu lỗi Parse thì in chuỗi gốc
                Console.WriteLine(payload);
            }

            // 3. In vạch kết thúc gói tin
            Console.WriteLine(new string('-', 60));
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string payload = e.ApplicationMessage.ConvertPayloadToString();
                //_logger.LogInformation($"Nhan goi tin tu MQTT: {payload}");
                WriteTerminal(payload);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var telemetryData = JsonSerializer.Deserialize<TelemetryMessage>(payload, options);

                if (telemetryData != null && telemetryData.Sensors.Count > 0)
                {
                    if (!DateTime.TryParse(telemetryData.Timestamp, out DateTime parsedTimestamp))
                    {
                        parsedTimestamp = DateTime.UtcNow;
                    }

                    // LUỒNG 1: LƯU LỊCH SỬ DATABASE
                    var dbTask = Task.Run(() =>
                    {
                        try
                        {
                            // Lưu dữ liệu vào SQL Server qua SqlProvider2
                            string sqlInsert = "INSERT INTO LichSuDo (ID_CamBien, GiaTri, ThoiGian) VALUES (@ID_CamBien, @GiaTri, @ThoiGian)";

                            foreach (var sensor in telemetryData.Sensors)
                            {
                                var parameters = new SqlParameter[]
                                {
                            new SqlParameter("@ID_CamBien", sensor.SensorId),
                            new SqlParameter("@GiaTri", sensor.Value),
                            new SqlParameter("@ThoiGian", parsedTimestamp)
                                };

                                _dbProvider.ExecuteNonQuery(sqlInsert, parameters);
                            }
                        }
                        catch (Exception dbEx)
                        {
                            _logger.LogError(dbEx, "Loi ghi database.");
                        }
                    });

                    // LUỒNG 2: KIỂM TRA BÁO ĐỘNG
                    var alertTask = Task.Run(async () =>
                    {
                        try
                        {
                            await _alertService.CheckAndFireAlertsAsync(telemetryData);
                        }
                        catch (Exception alertEx)
                        {
                            _logger.LogError(alertEx, "Loi xu ly Canh bao.");
                        }
                    });

                    // Hệ thống phải chạy 2 công việc trên cùng một lúc và đợi hoàn thành
                    await Task.WhenAll(dbTask, alertTask);

                    // Đẩy dữ liệu Real-time lên App WPF thông qua SignalR
                    //await _hubContext.Clients.Group($"Apartment_{telemetryData.ApartmentId}")
                    //    .SendAsync("ReceiveTelemetry", telemetryData);
                    await _hubContext.Clients.All.SendAsync("ReceiveTelemetry", telemetryData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi xu ly tin nhan MQTT.");
            }
        }
    }
}