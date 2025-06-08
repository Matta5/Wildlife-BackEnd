namespace Wildlife_BLL.DTO;

public class CreateEditObservationDTO
{
    public int SpeciesId { get; set; }
    public int UserId { get; set; }
    public string? Body { get; set; } = string.Empty;
    public DateTime? DateObserved { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
