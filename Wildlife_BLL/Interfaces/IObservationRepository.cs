using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces;

public interface IObservationRepository
{
    List<ObservationDTO> GetObservationsByUser(int userId);
    int CreateObservation(CreateObservationDTO observation);
    ObservationDTO? GetObservationById(int id);
    bool DeleteObservation(int id);
    bool PatchObservation(int id, PatchObservationDTO dto);
    
    // Statistics methods
    int GetTotalObservationsByUser(int userId);
    int GetUniqueSpeciesCountByUser(int userId);
    
    // Get all observations with limit
    List<ObservationDTO> GetAllObservations(int limit = 30, int? currentUserId = null, bool excludeCurrentUser = false);
}
