// Wildlife_BLL/Services/SpeciesService.cs
using System.Text.Json;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;

namespace Wildlife_BLL;

public class SpeciesService
{
    private readonly ISpeciesRepository _speciesRepository;
    private readonly IExternalSpeciesClient _externalSpeciesClient;

    public SpeciesService(ISpeciesRepository speciesRepository, IExternalSpeciesClient externalSpeciesClient)
    {
        _speciesRepository = speciesRepository;
        _externalSpeciesClient = externalSpeciesClient;
    }

    public async Task<SpeciesDTO?> GetSpeciesByIdAsync(int id)
    {
        return await _speciesRepository.GetByIdAsync(id);
    }

    public async Task<List<SpeciesDTO>> SearchSpeciesAsync(string searchTerm, int limit = 20)
    {
        var species = await _speciesRepository.SearchAsync(searchTerm, limit);
        return species;
    }

    public async Task<List<SpeciesDTO>> FindSpeciesAsync(string searchTerm, int limit = 10)
    {
        // 1. Search locally first
        var localResults = await _speciesRepository.SearchAsync(searchTerm, limit);
        
        // 2. If we have enough local results, return them
        if (localResults.Count >= limit)
            return localResults;

        // 3. Only search iNaturalist for queries with sufficient length (prevent spam)
        if (searchTerm.Length < 4)
        {
            return localResults; // Return only local results for short queries
        }

        // 4. Search iNaturalist API for additional results
        var apiSpecies = await _externalSpeciesClient.SearchByNameAsync(searchTerm);
        if (apiSpecies != null)
        {
            // Check if this species already exists locally
            var existing = await _speciesRepository.GetByTaxonIdAsync(apiSpecies.InaturalistTaxonId);
            if (existing == null)
            {
                // Convert CreateSpeciesDTO to SpeciesDTO for display (without saving to DB)
                var displaySpecies = new SpeciesDTO
                {
                    Id = 0, // Temporary ID for display
                    InaturalistTaxonId = apiSpecies.InaturalistTaxonId,
                    ScientificName = apiSpecies.ScientificName,
                    CommonName = apiSpecies.CommonName,
                    ImageUrl = apiSpecies.ImageUrl,
                    IconicTaxonName = apiSpecies.IconicTaxonName,
                    Taxonomy = new TaxonomyDTO
                    {
                        IconicTaxon = apiSpecies.IconicTaxonName,
                        Kingdom = apiSpecies.KingdomName,
                        Phylum = apiSpecies.PhylumName,
                        Class = apiSpecies.ClassName,
                        Order = apiSpecies.OrderName,
                        Family = apiSpecies.FamilyName,
                        Genus = apiSpecies.GenusName,
                        Species = apiSpecies.SpeciesName
                    }
                };
                localResults.Add(displaySpecies);
            }
        }

        return localResults.Take(limit).ToList();
    }

    public async Task<SpeciesDTO?> ImportSpeciesAsync(long taxonId)
    {
        // Check if we already have this species
        var existing = await _speciesRepository.GetByTaxonIdAsync(taxonId);
        if (existing != null)
            return existing;

        // Fetch from iNaturalist
        var species = await _externalSpeciesClient.GetByTaxonIdAsync(taxonId);
        if (species != null)
        {
            var savedSpecies = await _speciesRepository.AddAsync(species);
            return savedSpecies;
        }

        return null;
    }

    public async Task<SpeciesDTO?> ImportSpeciesByTaxonIdAsync(long taxonId)
    {
        return await ImportSpeciesAsync(taxonId);
    }

    public async Task<List<SpeciesDTO>> GetPopularDutchSpeciesAsync(int limit = 50)
    {
        var species = await _speciesRepository.GetPreloadedSpeciesAsync(limit);
        return species;
    }

    public async Task<List<SpeciesDTO>> GetSpeciesByClassAsync(string className, int limit = 20)
    {
        var species = await _speciesRepository.GetByClassificationAsync("class", className, limit);
        return species;
    }

    public async Task<List<SpeciesDTO>> GetSpeciesByOrderAsync(string orderName, int limit = 20)
    {
        var species = await _speciesRepository.GetByClassificationAsync("order", orderName, limit);
        return species;
    }

    public async Task<List<SpeciesDTO>> GetSpeciesByFamilyAsync(string familyName, int limit = 20)
    {
        var species = await _speciesRepository.GetByClassificationAsync("family", familyName, limit);
        return species;
    }
}