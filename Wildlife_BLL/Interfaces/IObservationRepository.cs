using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces;

public interface IObservationRepository
{
    void CreateObservation(CreateObservationDTO observation);
    bool DeleteObservation(int id);
    ObservationDTO? GetObservationById(int id);
    List<ObservationDTO> GetObservationsByUser(int userId);
    bool PatchObservation(int value, PatchObservationDTO dto);
    
    // Statistics methods
    int GetTotalObservationsByUser(int userId);
    int GetUniqueSpeciesCountByUser(int userId);
    
    // Get all observations with limit
    List<ObservationDTO> GetAllObservations(int limit = 30, int? currentUserId = null, bool excludeCurrentUser = false);
}
