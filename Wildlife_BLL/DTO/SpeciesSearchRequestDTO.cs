namespace Wildlife_BLL.DTO;

public class SpeciesSearchRequestDTO
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 20;
    public bool ImportIfNotFound { get; set; } = false;
} 