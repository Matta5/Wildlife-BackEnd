using System.Security.Cryptography.X509Certificates;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Wildlife_DAL
{
    public class ObservationRepository : IObservationRepository
    {
        private readonly AppDbContext _context;
        public ObservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<ObservationDTO> GetObservationsByUser(int userId)
        {
            var observations = _context.Observations
                .Include(o => o.User)
                .Include(o => o.Species)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.DatePosted)
                .Select(MapToObservationDTO)
                .ToList();
            return observations;
        }

        public int CreateObservation(CreateObservationDTO observation)
        {
            try
            {
                // Convert DateTime to UTC properly
                DateTime? dateObserved = null;
                if (observation.DateObserved.HasValue)
                {
                    var date = observation.DateObserved.Value;
                    if (date.Kind == DateTimeKind.Unspecified)
                    {
                        // Assume local time and convert to UTC
                        dateObserved = DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
                    }
                    else if (date.Kind == DateTimeKind.Local)
                    {
                        // Convert local to UTC
                        dateObserved = date.ToUniversalTime();
                    }
                    else
                    {
                        // If already UTC, use as is
                        dateObserved = date;
                    }
                }

                var entity = new ObservationEntity
                {
                    SpeciesId = observation.SpeciesId,
                    UserId = observation.UserId,
                    Body = observation.Body,
                    DateObserved = dateObserved,
                    Latitude = observation.Latitude,
                    Longitude = observation.Longitude,
                    ImageUrl = observation.ImageUrl
                };

                Console.WriteLine($"Creating observation: SpeciesId={entity.SpeciesId}, UserId={entity.UserId}, Body={entity.Body}, DateObserved={entity.DateObserved}, Lat={entity.Latitude}, Lng={entity.Longitude}");

                _context.Observations.Add(entity);
                _context.SaveChanges();
                
                return entity.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Database error creating observation: {e.Message}");
                Console.WriteLine($"Inner exception: {e.InnerException?.Message}");
                throw new Exception($"Failed to create observation: {e.Message}", e);
            }
        }

        public ObservationDTO? GetObservationById(int id)
        {
            var observation = _context.Observations
                .Include(o => o.User)
                .Include(o => o.Species)
                .Where(o => o.Id == id)
                .Select(MapToObservationDTO)
                .FirstOrDefault();

            return observation;
        }

        public bool DeleteObservation(int id)
        {
            var observation = _context.Observations.Find(id);
            if (observation != null)
            {
                _context.Observations.Remove(observation);
                _context.SaveChanges();
                return true;
            }
            else
            {
                throw new Exception("Observation not found");
            }
        }

        public bool PatchObservation(int id, PatchObservationDTO dto)
        {
            var observation = _context.Observations
                .FirstOrDefault(o => o.Id == id);

            if (observation == null)
                throw new Exception("Observation not found");

            if (!string.IsNullOrEmpty(dto.Body))
                observation.Body = dto.Body;
            if (dto.DateObserved != null)
            {
                observation.DateObserved = ConvertToUtc(dto.DateObserved);
            }
            if (dto.Latitude != null)
                observation.Latitude = dto.Latitude.Value;
            if (dto.Longitude != null)
                observation.Longitude = dto.Longitude.Value;
            if (!string.IsNullOrEmpty(dto.ImageUrl))
                observation.ImageUrl = dto.ImageUrl;

            _context.SaveChanges();
            return true;
        }

        public int GetTotalObservationsByUser(int userId)
        {
            return _context.Observations
                .Where(o => o.UserId == userId)
                .Count();
        }

        public int GetUniqueSpeciesCountByUser(int userId)
        {
            return _context.Observations
                .Where(o => o.UserId == userId)
                .Select(o => o.SpeciesId)
                .Distinct()
                .Count();
        }

        public List<ObservationDTO> GetAllObservations(int limit = 30, int? currentUserId = null, bool excludeCurrentUser = false)
        {
            var query = _context.Observations
                .Include(o => o.User)
                .Include(o => o.Species)
                .AsQueryable();
            
            // Filter out current user's observations if requested
            if (excludeCurrentUser && currentUserId.HasValue)
            {
                query = query.Where(o => o.UserId != currentUserId.Value);
            }
            
            var observations = query
                .OrderByDescending(o => o.DatePosted)
                .Take(limit)
                .Select(MapToObservationDTO)
                .ToList();
            return observations;
        }

        // Helper method to map ObservationEntity to ObservationDTO
        private static ObservationDTO MapToObservationDTO(ObservationEntity o)
        {
            return new ObservationDTO
            {
                Id = o.Id,
                SpeciesId = o.SpeciesId,
                UserId = o.UserId,
                Body = o.Body,
                DateObserved = o.DateObserved,
                DatePosted = o.DatePosted,
                Latitude = o.Latitude,
                Longitude = o.Longitude,
                ImageUrl = o.ImageUrl,
                User = o.User != null ? new MinimalUserDTO
                {
                    Username = o.User.Username,
                    ProfilePicture = o.User.ProfilePicture,
                } : null,
                Species = o.Species != null ? new SpeciesDTO
                {
                    Id = o.Species.Id,
                    CommonName = o.Species.CommonName,
                    ScientificName = o.Species.ScientificName,
                    InaturalistTaxonId = o.Species.InaturalistTaxonId,
                    ImageUrl = o.Species.ImageUrl,
                    IconicTaxonName = o.Species.IconicTaxonName,
                    Taxonomy = new TaxonomyDTO
                    {
                        IconicTaxon = o.Species.IconicTaxonName,
                        Kingdom = o.Species.KingdomName,
                        Phylum = o.Species.PhylumName,
                        Class = o.Species.ClassName,
                        Order = o.Species.OrderName,
                        Family = o.Species.FamilyName,
                        Genus = o.Species.GenusName,
                    }
                } : null
            };
        }

        // Helper method to convert DateTime to UTC
        private static DateTime? ConvertToUtc(DateTime? date)
        {
            if (!date.HasValue)
                return null;

            var dateValue = date.Value;
            if (dateValue.Kind == DateTimeKind.Unspecified)
            {
                // Assume local time and convert to UTC
                return DateTime.SpecifyKind(dateValue, DateTimeKind.Local).ToUniversalTime();
            }
            else if (dateValue.Kind == DateTimeKind.Local)
            {
                // Convert local to UTC
                return dateValue.ToUniversalTime();
            }
            else
            {
                // If already UTC, use as is
                return dateValue;
            }
        }
    }
}
