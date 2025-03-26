using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_DAL.Entities;

namespace Wildlife_DAL
{
    public class ObservationRepository : IObservationRepository
    {
        private readonly AppDbContext _context;
        public ObservationRepository(AppDbContext context) 
        {
            _context = context;
        }

        public List<ObservationDTO> GetObservations()
        {
            return new List<ObservationDTO>();
        }

        public List<ObservationDTO> GetObservationsByUser(int userId)
        {
            return new List<ObservationDTO>();
        }

        public void CreateObservation(CreateEditObservationDTO observation)
        {
            try
            {
                _context.Observations.Add(new ObservationEntity
                {
                    SpeciesId = observation.SpeciesId,
                    UserId = observation.UserId,
                    Body = observation.Body,
                    DateObserved = observation.DateObserved,
                    Latitude = observation.Latitude,
                    Longitude = observation.Longitude
                });
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create observation", e);
            }
        }

        public ObservationDTO? GetObservationById(int id)
        {
            return new ObservationDTO();
        }

    }
}
