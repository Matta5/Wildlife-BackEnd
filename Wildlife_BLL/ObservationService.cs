using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;

namespace Wildlife_BLL
{
    public class ObservationService
    {
        private readonly IObservationRepository _observationRepository;

        public ObservationService(IObservationRepository observationRepository)
        {
            _observationRepository = observationRepository;
        }

        public void CreateObservation(int userId, CreateEditObservationDTO dto)
        {
            CreateEditObservationDTO createEditObservationDTO = new CreateEditObservationDTO
            {
                SpeciesId = dto.SpeciesId,
                UserId = userId,
                Body = dto.Body,
                DateObserved = dto.DateObserved,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };
            _observationRepository.CreateObservation(createEditObservationDTO);
        }

        public ObservationDTO? GetObservationById(int id)
        {
            return _observationRepository.GetObservationById(id);
        }

        public List<ObservationDTO> GetObservationsByUser(int userId)
        {
            return _observationRepository.GetObservationsByUser(userId);
        }
    }

}