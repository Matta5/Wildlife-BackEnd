using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces;

public interface IObservationRepository
{
    void CreateObservation(CreateObservationDTO observation);
    bool DeleteObservation(int id);
    ObservationDTO? GetObservationById(int id);
    List<ObservationDTO> GetObservationsByUser(int userId);
    bool PatchObservation(int value, PatchObservationDTO dto);
}
