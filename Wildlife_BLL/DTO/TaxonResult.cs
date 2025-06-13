using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.DTO
{
    public class TaxonResult
    {
        public string? PreferredEnglishName { get; set; }
        public string? ScientificName { get; set; }
        public double Confidence { get; set; }
        
        // Additional taxon information for import
        public long? TaxonId { get; set; }
        public string? IconicTaxonName { get; set; }
        public string? Rank { get; set; }
        public string? ImageUrl { get; set; }
    }
}
