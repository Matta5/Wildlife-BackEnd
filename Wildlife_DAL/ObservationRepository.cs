using System.Security.Cryptography.X509Certificates;
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

        public List<ObservationDTO> GetObservationsByUser(int userId)
        {
            var observations = _context.Observations
                .Where(o => o.UserId == userId)
                .Select(o => new ObservationDTO
                {
                    Id = o.Id,
                    SpeciesId = o.SpeciesId,
                    UserId = o.UserId,
                    Body = o.Body,
                    DateObserved = o.DateObserved,
                    Latitude = o.Latitude,
                    Longitude = o.Longitude,
                    ImageUrl = o.ImageUrl
                })
                .ToList();
            return observations;
        }

        public void CreateObservation(CreateObservationDTO observation)
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
                    Longitude = observation.Longitude,
                    ImageUrl = observation.ImageUrl
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
            var observation = _context.Observations
                .Where(o => o.Id == id)
                .Select(o => new ObservationDTO
                {
                    Id = o.Id,
                    SpeciesId = o.SpeciesId,
                    UserId = o.UserId,
                    Body = o.Body,
                    DateObserved = o.DateObserved,
                    Latitude = o.Latitude,
                    Longitude = o.Longitude,
                    ImageUrl = o.ImageUrl,
                    User = new MinimalUserDTO
                    {
                        Username = o.User.Username,
                        ProfilePicture = o.User.ProfilePicture,
                    },
                    Species = new SpeciesDTO
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
                    }
                })
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
                observation.DateObserved = dto.DateObserved.Value;
            if (dto.Latitude != null)
                observation.Latitude = dto.Latitude.Value;
            if (dto.Longitude != null)
                observation.Longitude = dto.Longitude.Value;
            if (!string.IsNullOrEmpty(dto.ImageUrl))
                observation.ImageUrl = dto.ImageUrl;

            _context.SaveChanges();
            return true;
        }
    }
}
