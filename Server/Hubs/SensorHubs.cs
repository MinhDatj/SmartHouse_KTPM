using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SmartBuilding.Server.Hubs
{
    public class SensorHub : Hub
    {
        public async Task JoinApartmentGroup(string apartmentId)
        {
            await base.Groups.AddToGroupAsync(base.Context.ConnectionId, $"Apartment_{apartmentId}");
        }
    }
}