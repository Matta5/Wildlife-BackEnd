using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;
using Xunit;

namespace Wildlife_Tests.API_Tests;

public class SpeciesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SpeciesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> EnsureTestSpeciesAsync()
    {
        var response = await _client.PostAsync("/api/species/import/48978", null); // House Sparrow
        response.EnsureSuccessStatusCode();
        var species = await response.Content.ReadFromJsonAsync<SpeciesDTO>();
        return species?.Id ?? 1;
    }

    [Fact]
    public async Task GetSpecies_WithValidId_ReturnsOk()
    {
        // Arrange
        var speciesId = await EnsureTestSpeciesAsync();
        // Act
        var response = await _client.GetAsync($"/api/species/{speciesId}");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSpecies_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/species/99999");
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchSpecies_WithValidQuery_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/search?q=bird&limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task SearchSpecies_WithEmptyQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/species/search?q=");
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchSpecies_WithWhitespaceQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/species/search?q=%20%20");
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchSpecies_WithHighLimit_RespectsMaximum()
    {
        // Act
        var response = await _client.GetAsync("/api/species/search?q=bird&limit=200");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
        Assert.True(species.Count <= 100); // Should be capped at 100
    }

    [Fact]
    public async Task FindSpecies_WithValidQuery_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/find?q=eagle&limit=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task FindSpecies_WithEmptyQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/species/find?q=");
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FindSpecies_WithHighLimit_RespectsMaximum()
    {
        // Act
        var response = await _client.GetAsync("/api/species/find?q=bird&limit=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
        Assert.True(species.Count <= 50); // Should be capped at 50
    }

    [Fact]
    public async Task FindSpecies_WithNoResults_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/species/find?q=nonexistentspecies12345&limit=5");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("No species found", content);
    }

    [Fact]
    public async Task ImportSpecies_WithValidTaxonId_ReturnsOk()
    {
        // Act - Using a known iNaturalist taxon ID for a common species
        var response = await _client.PostAsync("/api/species/import/48978", null); // House Sparrow

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<SpeciesDTO>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task ImportSpecies_WithInvalidTaxonId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync("/api/species/import/999999999", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not found", content);
    }

    [Fact]
    public async Task DebugInaturalist_WithValidTaxonId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/debug/inaturalist/48978");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetPopularSpecies_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/popular?limit=20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task GetPopularSpecies_WithHighLimit_RespectsMaximum()
    {
        // Act
        var response = await _client.GetAsync("/api/species/popular?limit=500");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
        Assert.True(species.Count <= 200); // Should be capped at 200
    }

    [Fact]
    public async Task GetSpeciesByClass_WithValidClass_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/class/Aves?limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task GetSpeciesByClass_WithEmptyClass_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/species/class/?limit=10");
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSpeciesByClass_WithHighLimit_RespectsMaximum()
    {
        // Act
        var response = await _client.GetAsync("/api/species/class/Aves?limit=200");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
        Assert.True(species.Count <= 100); // Should be capped at 100
    }

    [Fact]
    public async Task GetSpeciesByOrder_WithValidOrder_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/order/Passeriformes?limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task GetSpeciesByOrder_WithEmptyOrder_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/species/order/?limit=10");
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSpeciesByFamily_WithValidFamily_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/species/family/Corvidae?limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var species = await response.Content.ReadFromJsonAsync<List<SpeciesDTO>>();
        Assert.NotNull(species);
    }

    [Fact]
    public async Task GetSpeciesByFamily_WithEmptyFamily_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/species/family/?limit=10");
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}