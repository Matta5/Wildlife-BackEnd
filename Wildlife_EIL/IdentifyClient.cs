using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;

namespace Wildlife_EIL;

public class IdentifyClient : IIdentifyClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl = "https://api.inaturalist.org/v1/computervision/score_image";

    public IdentifyClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var apiToken = configuration["iNaturalist:ApiToken"];

        if (!string.IsNullOrEmpty(apiToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiToken);
        }
    }

    public async Task<IdentifyResponseDTO> IdentifyAsync(byte[] imageBytes, double? latitude = null, double? longitude = null)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(imageBytes), "image", "uploaded_image.jpg");

            // Add location if provided (improves accuracy)
            if (latitude.HasValue && longitude.HasValue)
            {
                form.Add(new StringContent(latitude.Value.ToString()), "lat");
                form.Add(new StringContent(longitude.Value.ToString()), "lng");
            }

            var response = await _httpClient.PostAsync(_apiUrl, form);

            if (!response.IsSuccessStatusCode)
            {
                return new IdentifyResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"API returned {response.StatusCode}: {response.ReasonPhrase}"
                };
            }

            var responseString = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseString);
        }
        catch (HttpRequestException ex)
        {
            return new IdentifyResponseDTO
            {
                Success = false,
                ErrorMessage = $"Network error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new IdentifyResponseDTO
            {
                Success = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    private IdentifyResponseDTO ParseResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return new IdentifyResponseDTO
                {
                    Success = false,
                    ErrorMessage = "No identification results found"
                };
            }

            var taxonResults = new List<TaxonResult>();
            var firstResult = results[0];

            // Parse all results for alternatives
            foreach (var result in results.EnumerateArray().Take(5)) // Top 5 results
            {
                if (result.TryGetProperty("taxon", out var taxon))
                {
                    var commonName = taxon.TryGetProperty("preferred_common_name", out var nameEl)
                        ? nameEl.GetString() : "Unknown";
                    var scientificName = taxon.TryGetProperty("name", out var sciEl)
                        ? sciEl.GetString() : null;
                    var confidence = result.TryGetProperty("combined_score", out var scoreEl)
                        ? scoreEl.GetDouble() : 0.0;
                    
                    // Extract taxon ID for import
                    var taxonId = taxon.TryGetProperty("id", out var idEl)
                        ? idEl.GetInt64() : (long?)null;
                    
                    // Extract additional taxon information
                    var iconicTaxonName = taxon.TryGetProperty("iconic_taxon_name", out var iconicEl)
                        ? iconicEl.GetString() : null;
                    var rank = taxon.TryGetProperty("rank", out var rankEl)
                        ? rankEl.GetString() : null;
                    
                    // Extract default photo if available
                    string? imageUrl = null;
                    if (taxon.TryGetProperty("default_photo", out var photoEl))
                    {
                        imageUrl = photoEl.TryGetProperty("medium_url", out var urlEl)
                            ? urlEl.GetString() : null;
                    }

                    taxonResults.Add(new TaxonResult
                    {
                        PreferredEnglishName = commonName,
                        ScientificName = scientificName,
                        Confidence = Math.Round(confidence * 100, 2), // Convert to percentage
                        TaxonId = taxonId,
                        IconicTaxonName = iconicTaxonName,
                        Rank = rank,
                        ImageUrl = imageUrl
                    });
                }
            }

            var topResult = taxonResults.FirstOrDefault();
            return new IdentifyResponseDTO
            {
                Success = true,
                PreferredEnglishName = topResult?.PreferredEnglishName ?? "Unknown",
                ScientificName = topResult?.ScientificName,
                Confidence = topResult?.Confidence,
                AlternativeResults = taxonResults.ToList() // Include all results including the first one
            };
        }
        catch (JsonException ex)
        {
            return new IdentifyResponseDTO
            {
                Success = false,
                ErrorMessage = $"Failed to parse API response: {ex.Message}"
            };
        }
    }
}