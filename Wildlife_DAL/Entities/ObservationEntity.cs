using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_DAL.Entities
{
    public class ObservationEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? SpeciesId { get; set; } 
        public string? Body { get; set; } = string.Empty;
        public DateTime? DateObserved { get; set; }
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ImageUrl { get; set; } = string.Empty;

        public UserEntity User { get; set; } = null!;
    }

}
