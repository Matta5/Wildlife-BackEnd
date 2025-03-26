using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces;

public interface IObservationRepository
{
    void CreateObservation(CreateEditObservationDTO observation);
    ObservationDTO? GetObservationById(int id);
    List<ObservationDTO> GetObservations();
    List<ObservationDTO> GetObservationsByUser(int userId);
}
