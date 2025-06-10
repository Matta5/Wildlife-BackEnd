namespace Wildlife_BLL.DTO;

public class SpeciesDTO
{
    public int Id { get; set; }
    public long InaturalistTaxonId { get; set; }
    public string? ScientificName { get; set; }
    public string? CommonName { get; set; }
    public string? ImageUrl { get; set; }
    public string? IconicTaxonName { get; set; }

    public TaxonomyDTO? Taxonomy { get; set; }
    public List<ObservationDTO> Observations { get; set; } = new List<ObservationDTO>();
}