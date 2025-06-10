using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces
{
    public interface ISpeciesRepository
    {
        Task<SpeciesDTO?> GetByIdAsync(int id);
        Task<SpeciesDTO?> GetByTaxonIdAsync(long taxonId);
        Task<List<SpeciesDTO>> SearchAsync(string searchTerm, int limit = 20);
        Task<bool> ExistsAsync(long taxonId);
        Task<List<SpeciesDTO>> GetByClassificationAsync(string classification, string value, int limit = 20);
        Task<SpeciesDTO> AddAsync(CreateSpeciesDTO species);
        Task<List<SpeciesDTO>> GetPreloadedSpeciesAsync(int limit = 50);
    }
}
