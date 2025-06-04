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
    }
}
