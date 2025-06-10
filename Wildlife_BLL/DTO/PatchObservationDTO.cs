using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.DTO
{
    public class PatchObservationDTO
    {
        public string? Body { get; set; }
        public DateTime? DateObserved { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? SpeciesId { get; set; }
        public string? ImageUrl { get; set; }
        public string? IconicTaxonName { get; set; }
    }
}
