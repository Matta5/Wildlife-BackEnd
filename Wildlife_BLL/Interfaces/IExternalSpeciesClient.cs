using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces;

public interface IExternalSpeciesClient
{
    Task<CreateSpeciesDTO?> SearchByNameAsync(string name);
    Task<CreateSpeciesDTO?> GetByTaxonIdAsync(long taxonId);
} 