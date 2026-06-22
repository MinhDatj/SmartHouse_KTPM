using Microsoft.AspNetCore.SignalR;
using SmartBuilding.Server.DTOs;
using System.Threading.Tasks;

namespace SmartBuilding.Server.Hubs
{
    public class SensorHub : Hub
    {
        public async Task JoinApartmentGroup(int apartmentId)
        {
            await base.Groups.AddToGroupAsync(base.Context.ConnectionId, $"Apartment_{apartmentId}");
        }

        public async Task RelayAlertToManager(int apartmentId, string message)
        {
            // Đóng gói lại thành Object AlertMessage và phát thanh xuống App Manager
            var alert = new AlertMessage
            {
                ApartmentId = apartmentId,
                ApartmentName = $"Căn hộ 10{apartmentId}", // Tự động map ID 2 thành "Căn hộ 102"
                Message = message,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                WarningLevel = "SOS",
                SensorType = "Thủ công (App)", // Đánh dấu là do chủ hộ tự bấm
                AlertValue = 0
            };

            // Gửi sự kiện "ReceiveAlert" tới tất cả client (hoặc group Manager)
            await Clients.All.SendAsync("ReceiveAlert", alert);
        }
    }
}