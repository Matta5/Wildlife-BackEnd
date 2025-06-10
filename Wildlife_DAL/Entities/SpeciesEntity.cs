namespace Wildlife_DAL.Entities;

public class SpeciesEntity
{
    public int Id { get; set; }
    public long InaturalistTaxonId { get; set; }
    public string? ScientificName { get; set; }
    public string? CommonName { get; set; }
    public string? ImageUrl { get; set; }

    public string? IconicTaxonName { get; set; } 
    public string? KingdomName { get; set; }
    public string? PhylumName { get; set; }
    public string? ClassName { get; set; } 
    public string? OrderName { get; set; }
    public string? FamilyName { get; set; }
    public string? GenusName { get; set; }
    public string? SpeciesName { get; set; }

    public List<ObservationEntity> Observations { get; set; } = new List<ObservationEntity>();
}