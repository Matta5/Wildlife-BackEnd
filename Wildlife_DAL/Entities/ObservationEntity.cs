namespace Wildlife_DAL.Entities;

public class ObservationEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SpeciesId { get; set; } 

    public string? Body { get; set; }
    public DateTime? DateObserved { get; set; }
    public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }

    public UserEntity User { get; set; } = null!;
    public SpeciesEntity Species { get; set; } = null!;
}
