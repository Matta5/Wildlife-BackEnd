namespace Wildlife_BLL.DTO;

public class SpeciesSearchResponseDTO
{
    public List<SpeciesDTO> Results { get; set; } = new List<SpeciesDTO>();
    public int TotalCount { get; set; }
    public bool ImportedFromInaturalist { get; set; } = false;
    public string? Message { get; set; }
} 