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
            // According to iNaturalist API docs: GET /v1/taxa with query parameters
            var url = $"https://api.inaturalist.org/v1/taxa?q={Uri.EscapeDataString(name)}&rank=species&per_page=1";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            var results = json.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0)
                return null;

            var taxon = results[0];
            return MapJsonToDto(taxon);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchByNameAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<CreateSpeciesDTO?> GetByTaxonIdAsync(long taxonId)
    {
        try
        {
            // According to iNaturalist API docs: GET /v1/taxa/{id}
            var url = $"https://api.inaturalist.org/v1/taxa/{taxonId}";
            Console.WriteLine($"Calling iNaturalist API: {url}");
            
            var response = await _httpClient.GetAsync(url);
            Console.WriteLine($"Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {errorContent}");
                return null;
            }
            
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content length: {responseString.Length}");
            
            var json = JsonDocument.Parse(responseString);
            
            // According to API docs, the response should have a "results" array
            if (!json.RootElement.TryGetProperty("results", out var results))
            {
                Console.WriteLine("No 'results' property found in response");
                return null;
            }
            
            if (results.GetArrayLength() == 0)
            {
                Console.WriteLine("Results array is empty");
                return null;
            }

            var taxon = results[0];
            return MapJsonToDto(taxon);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP request error in GetByTaxonIdAsync: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error in GetByTaxonIdAsync: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in GetByTaxonIdAsync: {ex.Message}");
            return null;
        }
    }

    private CreateSpeciesDTO MapJsonToDto(JsonElement taxon)
    {
        try
        {
            Console.WriteLine($"Mapping taxon with ID: {taxon.GetProperty("id").GetInt64()}");
            
            var dto = new CreateSpeciesDTO
            {
                InaturalistTaxonId = taxon.GetProperty("id").GetInt64(),
                ScientificName = GetJsonProperty(taxon, "name"),
                CommonName = GetJsonProperty(taxon, "preferred_common_name"),
                ImageUrl = GetJsonProperty(taxon, "default_photo", "medium_url"),
                IconicTaxonName = GetJsonProperty(taxon, "iconic_taxon_name")
            };

            // Handle ancestors - they might not exist or be structured differently
            if (taxon.TryGetProperty("ancestors", out var ancestors))
            {
                Console.WriteLine($"Found ancestors array with {ancestors.GetArrayLength()} items");
                
                // Process each ancestor to find the taxonomic levels
                foreach (var ancestor in ancestors.EnumerateArray())
                {
                    if (ancestor.TryGetProperty("rank", out var rank) && ancestor.TryGetProperty("name", out var name))
                    {
                        var rankValue = rank.GetString();
                        var nameValue = name.GetString();
                        
                        switch (rankValue?.ToLower())
                        {
                            case "kingdom":
                                dto.KingdomName = nameValue;
                                break;
                            case "phylum":
                                dto.PhylumName = nameValue;
                                break;
                            case "class":
                                dto.ClassName = nameValue;
                                break;
                            case "order":
                                dto.OrderName = nameValue;
                                break;
                            case "family":
                                dto.FamilyName = nameValue;
                                break;
                            case "genus":
                                dto.GenusName = nameValue;
                                break;
                            case "species":
                                dto.SpeciesName = nameValue;
                                break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No ancestors property found in taxon");
            }

            return dto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error mapping JSON to DTO: {ex.Message}");
            throw;
        }
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