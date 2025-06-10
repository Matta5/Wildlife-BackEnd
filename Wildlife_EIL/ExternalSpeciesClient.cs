using System.Text.Json;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;

public class ExternalSpeciesClient : IExternalSpeciesClient
{
    private readonly HttpClient _httpClient;

    public ExternalSpeciesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateSpeciesDTO?> SearchByNameAsync(string name)
    {
        try
        {
            var url = $"https://api.inaturalist.org/v1/taxa?q={Uri.EscapeDataString(name)}&rank=species&per_page=1&locale=nl";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            var results = json.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0)
                return null;

            var taxon = results[0];
            return MapJsonToDto(taxon);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<CreateSpeciesDTO?> GetByTaxonIdAsync(long taxonId)
    {
        try
        {
            var url = $"https://api.inaturalist.org/v1/taxa/{taxonId}?locale=nl";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            var taxon = json.RootElement.GetProperty("results")[0];
            return MapJsonToDto(taxon);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private CreateSpeciesDTO MapJsonToDto(JsonElement taxon)
    {
        return new CreateSpeciesDTO
        {
            InaturalistTaxonId = taxon.GetProperty("id").GetInt64(),
            ScientificName = GetJsonProperty(taxon, "name"),
            CommonName = GetJsonProperty(taxon, "preferred_common_name"),
            ImageUrl = GetJsonProperty(taxon, "default_photo", "medium_url"),
            IconicTaxonName = GetJsonProperty(taxon, "iconic_taxon_name"),
            KingdomName = GetJsonProperty(taxon, "ancestors", "kingdom", "name"),
            PhylumName = GetJsonProperty(taxon, "ancestors", "phylum", "name"),
            ClassName = GetJsonProperty(taxon, "ancestors", "class", "name"),
            OrderName = GetJsonProperty(taxon, "ancestors", "order", "name"),
            FamilyName = GetJsonProperty(taxon, "ancestors", "family", "name"),
            GenusName = GetJsonProperty(taxon, "ancestors", "genus", "name"),
            SpeciesName = GetJsonProperty(taxon, "ancestors", "species", "name")
        };
    }

    private string? GetJsonProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
            return prop.GetString();
        return null;
    }

    private string? GetJsonProperty(JsonElement element, string propertyName, string nestedProperty)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.TryGetProperty(nestedProperty, out var nested))
            return nested.GetString();
        return null;
    }

    private string? GetJsonProperty(JsonElement element, string propertyName, string nestedProperty, string finalProperty)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.TryGetProperty(nestedProperty, out var nested) && nested.TryGetProperty(finalProperty, out var final))
            return final.GetString();
        return null;
    }
} 