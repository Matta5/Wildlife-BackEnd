namespace Wildlife_DAL.Entities;

public class SpeciesEntity
{
    public int Id { get; set; }
    public long InaturalistTaxonId { get; set; }
    public string ScientificName { get; set; }
    public string CommonName { get; set; }
    public string ImageUrl { get; set; }
    public List<ObservationEntity> Observations { get; set; } = new List<ObservationEntity>();
}
