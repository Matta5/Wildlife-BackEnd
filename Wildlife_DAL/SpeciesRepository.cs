using Microsoft.EntityFrameworkCore;
using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_DAL.Entities;
using Wildlife_BLL.DTO;

namespace Wildlife_DAL;

public class SpeciesRepository : ISpeciesRepository
{
    private readonly AppDbContext _context;

    public SpeciesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SpeciesDTO?> GetByIdAsync(int id)
    {
        var species = await _context.Species.FindAsync(id);
        if (species == null)
            return null;

        // Get observations for this species with user info
        var observations = await _context.Observations
            .Include(o => o.User)
            .Where(o => o.SpeciesId == id)
            .ToListAsync();

        var speciesDTO = MapToDto(species);
        // Map observations to DTO
        speciesDTO.Observations = observations.Select(MapObservationToDto).ToList();

        return speciesDTO;
    }

    public async Task<SpeciesDTO?> GetByTaxonIdAsync(long taxonId)
    {
        var species = await _context.Species
            .FirstOrDefaultAsync(s => s.InaturalistTaxonId == taxonId);
        
        return species != null ? MapToDto(species) : null;
    }

    public async Task<List<SpeciesDTO>> SearchAsync(string searchTerm, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<SpeciesDTO>();

        var term = searchTerm.ToLower().Trim();

        var species = await _context.Species
            .Where(s =>
                (s.CommonName != null && s.CommonName.ToLower().Contains(term)) ||
                (s.ScientificName != null && s.ScientificName.ToLower().Contains(term)) ||
                (s.GenusName != null && s.GenusName.ToLower().Contains(term)))
            .OrderByDescending(s => s.CommonName != null && s.CommonName.ToLower().StartsWith(term)) // Exact matches first
            .ThenByDescending(s => s.ScientificName != null && s.ScientificName.ToLower().StartsWith(term))
            .ThenBy(s => s.CommonName ?? s.ScientificName)
            .Take(limit)
            .ToListAsync();

        return species.Select(MapToDto).ToList();
    }

    public async Task<bool> ExistsAsync(long taxonId)
    {
        return await _context.Species
            .AnyAsync(s => s.InaturalistTaxonId == taxonId);
    }

    public async Task<List<SpeciesDTO>> GetByClassificationAsync(string classification, string value, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(classification) || string.IsNullOrWhiteSpace(value))
            return new List<SpeciesDTO>();

        var val = value.ToLower().Trim();

        var species = classification.ToLower() switch
        {
            "class" => await _context.Species
                .Where(s => s.ClassName != null && s.ClassName.ToLower() == val)
                .OrderBy(s => s.CommonName ?? s.ScientificName)
                .Take(limit)
                .ToListAsync(),

            "order" => await _context.Species
                .Where(s => s.OrderName != null && s.OrderName.ToLower() == val)
                .OrderBy(s => s.CommonName ?? s.ScientificName)
                .Take(limit)
                .ToListAsync(),

            "family" => await _context.Species
                .Where(s => s.FamilyName != null && s.FamilyName.ToLower() == val)
                .OrderBy(s => s.CommonName ?? s.ScientificName)
                .Take(limit)
                .ToListAsync(),

            "genus" => await _context.Species
                .Where(s => s.GenusName != null && s.GenusName.ToLower() == val)
                .OrderBy(s => s.CommonName ?? s.ScientificName)
                .Take(limit)
                .ToListAsync(),

            _ => new List<SpeciesEntity>()
        };

        return species.Select(MapToDto).ToList();
    }

    public async Task<SpeciesDTO> AddAsync(CreateSpeciesDTO speciesDto)
    {
        var species = new SpeciesEntity
        {
            InaturalistTaxonId = speciesDto.InaturalistTaxonId,
            ScientificName = speciesDto.ScientificName,
            CommonName = speciesDto.CommonName,
            ImageUrl = speciesDto.ImageUrl,
            IconicTaxonName = speciesDto.IconicTaxonName,
            KingdomName = speciesDto.KingdomName,
            PhylumName = speciesDto.PhylumName,
            ClassName = speciesDto.ClassName,
            OrderName = speciesDto.OrderName,
            FamilyName = speciesDto.FamilyName,
            GenusName = speciesDto.GenusName,
            SpeciesName = speciesDto.SpeciesName
        };

        _context.Species.Add(species);
        await _context.SaveChangesAsync();
        
        return MapToDto(species);
    }

    public async Task<List<SpeciesDTO>> GetPreloadedSpeciesAsync(int limit = 50)
    {
        var species = await _context.Species
            .OrderBy(s => s.CommonName ?? s.ScientificName)
            .Take(limit)
            .ToListAsync();

        return species.Select(MapToDto).ToList();
    }

    private SpeciesDTO MapToDto(SpeciesEntity species)
    {
        return new SpeciesDTO
        {
            Id = species.Id,
            CommonName = species.CommonName,
            ScientificName = species.ScientificName,
            InaturalistTaxonId = species.InaturalistTaxonId,
            ImageUrl = species.ImageUrl,
            IconicTaxonName = species.IconicTaxonName,
            Taxonomy = new TaxonomyDTO
            {
                IconicTaxon = species.IconicTaxonName,
                Kingdom = species.KingdomName,
                Phylum = species.PhylumName,
                Class = species.ClassName,
                Order = species.OrderName,
                Family = species.FamilyName,
                Genus = species.GenusName,
            }
        };
    }

    private ObservationDTO MapObservationToDto(ObservationEntity observation)
    {
        return new ObservationDTO
        {
            Id = observation.Id,
            UserId = observation.UserId,
            SpeciesId = observation.SpeciesId,
            Body = observation.Body,
            DateObserved = observation.DateObserved,
            DatePosted = observation.DatePosted,
            Latitude = observation.Latitude,
            Longitude = observation.Longitude,
            ImageUrl = observation.ImageUrl,
            User = MapUserToDto(observation.User)
        };
    }

    private MinimalUserDTO MapUserToDto(UserEntity user)
    {
        return new MinimalUserDTO
        {
            Username = user.Username,
            ProfilePicture = user.ProfilePicture,
        };
    }
}