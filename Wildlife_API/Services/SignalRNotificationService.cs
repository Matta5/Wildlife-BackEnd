using Microsoft.AspNetCore.SignalR;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Wildlife_API.Hubs;

namespace Wildlife_API.Services
{
    public class SignalRNotificationService : IObservationNotificationService
    {
        private readonly IHubContext<ObservationHub> _hubContext;

        public SignalRNotificationService(IHubContext<ObservationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyObservationCreated(ObservationDTO observation)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNewObservation", observation);
        }

        public async Task NotifyObservationUpdated(ObservationDTO observation)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveObservationUpdate", observation);
        }

        public async Task NotifyObservationDeleted(int observationId)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveObservationDelete", observationId);
        }
    }
} 