namespace Wildlife_BLL.DTO;

public class ObservationDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SpeciesId { get; set; }
    public string? Body { get; set; } = string.Empty;
    public DateTime? DateObserved { get; set; }
    public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }

    public MinimalUserDTO User { get; set; }
    public SpeciesDTO? Species { get; set; }
}