using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.DTO
{
    public class IdentifyResponseDTO
    {
        public bool Success { get; set; }
        public string? PreferredEnglishName { get; set; }
        public string? ScientificName { get; set; }
        public double? Confidence { get; set; }
        public string? ErrorMessage { get; set; }
        public List<TaxonResult>? AlternativeResults { get; set; }
        
        // Import-related properties
        public int? ImportedSpeciesId { get; set; }
        public string? ImportMessage { get; set; }
    }
}
