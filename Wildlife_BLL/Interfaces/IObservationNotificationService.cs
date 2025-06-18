using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces
{
    public interface IObservationNotificationService
    {
        Task NotifyObservationCreated(ObservationDTO observation);
        Task NotifyObservationUpdated(ObservationDTO observation);
        Task NotifyObservationDeleted(int observationId);
    }
} 