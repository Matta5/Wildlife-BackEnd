namespace Wildlife_BLL.DTO;

public class CreateObservationDTO
{
    public int SpeciesId { get; set; }
    public int UserId { get; set; }
    public string? Body { get; set; } = string.Empty;
    public DateTime? DateObserved { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }
}

public class CreateObservationFormDTO
{
    public string SpeciesId { get; set; } = "";
    public string? Body { get; set; } = string.Empty;
    public string? DateObserved { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }

    public CreateObservationDTO ToCreateObservationDTO(int userId)
    {
        return new CreateObservationDTO
        {
            SpeciesId = int.TryParse(SpeciesId, out int speciesId) ? speciesId : 0,
            UserId = userId,
            Body = Body,
            DateObserved = DateTime.TryParse(DateObserved, out DateTime date) ? date : null,
            Latitude = double.TryParse(Latitude, out double lat) ? lat : null,
            Longitude = double.TryParse(Longitude, out double lng) ? lng : null
        };
    }
}

public class CreateObservationSimpleDTO
{
    public int SpeciesId { get; set; }
    public string? Body { get; set; } = string.Empty;
    public DateTime? DateObserved { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
