using Microsoft.AspNetCore.SignalR;
using Wildlife_BLL.DTO;

namespace Wildlife_API.Hubs
{
    public class ObservationHub : Hub
    {
        public async Task NotifyNewObservation(ObservationDTO observation)
        {
            await Clients.All.SendAsync("ReceiveNewObservation", observation);
        }

        public async Task NotifyObservationUpdated(ObservationDTO observation)
        {
            await Clients.All.SendAsync("ReceiveObservationUpdate", observation);
        }

        public async Task NotifyObservationDeleted(int observationId)
        {
            await Clients.All.SendAsync("ReceiveObservationDelete", observationId);
        }
    }
} 