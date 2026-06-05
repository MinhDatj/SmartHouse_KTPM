using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using SmartBuilding.Server.Models;

namespace Server.Services;

public class AlertProcessingService
{
    private readonly IHubContext<ApartmentHub> _hubContext;

    public AlertProcessingService(IHubContext<ApartmentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task ProcessDataAsync(dynamic sensorData)
    {
        try
        {
            // 1. Lấy các chỉ số từ gói dữ liệu 
            double nhietDo = (double)sensorData.GetProperty("NhietDo").GetDouble();
            bool coKhoi = (bool)sensorData.GetProperty("Khoi").GetBoolean();
            string canHoId = sensorData.GetProperty("CanHoId").GetString() ?? "UNKNOWN";

            // 2. LOGIC KIỂM TRA ĐIỀU KIỆN
            bool isOverHeated = nhietDo > 60.0;
            bool hasSmoke = coKhoi;
            bool isDanger = isOverHeated || hasSmoke;

            // 3. XỬ LÝ GIAO DIỆN CONSOLE (In ra mọi lúc)
            string thoiGian = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            string trangThaiKhoi = coKhoi ? "Co khoi" : "Khong khoi";
            
            // Nếu nguy hiểm thì chữ màu Đỏ, bình thường thì chữ màu Xanh lá
            Console.ForegroundColor = isDanger ? ConsoleColor.Red : ConsoleColor.Green;
            
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine($"Vao luc: {thoiGian}");
            Console.WriteLine($"Can ho: {canHoId}");
            Console.WriteLine($"Nhiet do: {Math.Round(nhietDo, 1)} do C");
            Console.WriteLine($"Khoi: {trangThaiKhoi}");
            Console.WriteLine(isDanger ? ">> CANH BAO: Co the bi hoa hoan! <<" : ">> Trang thai: Binh thuong <<");
            Console.WriteLine("----------------------------------------");
            
            Console.ResetColor(); // Trả lại màu trắng mặc định cho các log khác

            // 4. BẮN SIGNALR (Chỉ bắn khi có nguy hiểm)
            if (isDanger)
            {
                var alert = new AlertPayload
                {
                    ApartmentId = canHoId,
                    AlertType = (isOverHeated && hasSmoke) ? "Fire" : (isOverHeated ? "Temperature" : "Smoke"),
                    CurrentValue = isOverHeated ? Math.Round(nhietDo, 2) : 1.0,
                    Message = $"CANH BAO NGUY HIEM: Can ho {canHoId} co dau hieu hoa hoan!",
                    Timestamp = DateTime.UtcNow,
                    Status = "DANGER"
                };

                string jsonAlert = JsonSerializer.Serialize(alert);
                await _hubContext.Clients.All.SendAsync("ReceiveEmergencyAlert", jsonAlert);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Loi xu ly canh bao]: {ex.Message}");
        }
    }
}