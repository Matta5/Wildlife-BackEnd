using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;
using Xunit;

namespace Wildlife_Tests.API_Tests;

public class ObservationControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ObservationControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(int UserId, string Username, string Email)> CreateTestUserAsync()
    {
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(username), "Username");
        form.Add(new StringContent(email), "Email");
        form.Add(new StringContent("password123"), "Password");

        var emptyFileContent = new ByteArrayContent(Array.Empty<byte>());
        emptyFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(emptyFileContent, "ProfilePicture", "test.jpg");

        var response = await _client.PostAsync("/users", form);
        response.EnsureSuccessStatusCode();

        // Get user ID by calling get all users
        var getAllResponse = await _client.GetAsync("/users");
        var users = await getAllResponse.Content.ReadFromJsonAsync<List<UserDTO>>();
        var createdUser = users?.FirstOrDefault(u => u.Username == username);
        var userId = createdUser?.Id ?? 0;

        return (userId, username, email);
    }

    private async Task<string> GetAuthTokenAsync(string username, string password)
    {
        var loginData = new LoginDTO
        {
            Username = username,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginData);
        loginResponse.EnsureSuccessStatusCode();

        var authHeader = loginResponse.Headers.GetValues("Authorization").FirstOrDefault();
        return authHeader?.Replace("Bearer ", "") ?? "";
    }

    private async Task<int> CreateTestSpeciesAsync()
    {
        // Import a known species to get a valid species ID
        var response = await _client.PostAsync("/api/species/import/48978", null); // House Sparrow
        response.EnsureSuccessStatusCode();
        var species = await response.Content.ReadFromJsonAsync<SpeciesDTO>();
        return species?.Id ?? 1; // Fallback to 1 if import fails
    }

    private void SetAuthToken(string token)
    {
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    [Fact]
    public async Task GetObservationsByUser_WithValidUser_ReturnsOk()
    {
        // Arrange
        var (userId, username, email) = await CreateTestUserAsync();

        // Act
        var response = await _client.GetAsync($"/observations/GetAllFromUser/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        Assert.NotNull(observations);
    }

    [Fact]
    public async Task GetObservationsByUser_WithInvalidUser_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/observations/GetAllFromUser/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("No observations found", content);
    }

    [Fact]
    public async Task GetAllObservations_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/observations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        Assert.NotNull(observations);
    }

    [Fact]
    public async Task GetAllObservations_WithLimit_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/observations?limit=15");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        Assert.NotNull(observations);
        Assert.True(observations.Count <= 15);
    }

    [Fact]
    public async Task GetAllObservations_WithExcludeCurrentUser_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/observations?excludeCurrentUser=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        Assert.NotNull(observations);
    }

    [Fact]
    public async Task GetDiscoverObservations_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/observations/discover");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        Assert.NotNull(observations);
    }

    [Fact]
    public async Task GetExploreObservations_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/observations/explore");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observations = await response.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        Assert.NotNull(observations);
    }

    [Fact]
    public async Task CreateObservation_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("1"), "SpeciesId");
        form.Add(new StringContent("Test observation"), "Body");
        form.Add(new StringContent(DateTime.UtcNow.ToString("yyyy-MM-dd")), "DateObserved");
        form.Add(new StringContent("52.3676"), "Latitude");
        form.Add(new StringContent("4.9041"), "Longitude");

        // Act
        var response = await _client.PostAsync("/observations", form);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateObservation_WithValidData_ReturnsOk()
    {
        // Arrange
        var (userId, username, email) = await CreateTestUserAsync();
        var token = await GetAuthTokenAsync(username, "password123");
        var speciesId = await CreateTestSpeciesAsync();
        SetAuthToken(token);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(speciesId.ToString()), "SpeciesId");
        form.Add(new StringContent("Test observation"), "Body");
        form.Add(new StringContent(DateTime.UtcNow.ToString("yyyy-MM-dd")), "DateObserved");
        form.Add(new StringContent("52.3676"), "Latitude");
        form.Add(new StringContent("4.9041"), "Longitude");

        // Act
        var response = await _client.PostAsync("/observations", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateObservation_WithInvalidSpecies_ReturnsBadRequest()
    {
        // Arrange
        var (userId, username, email) = await CreateTestUserAsync();
        var token = await GetAuthTokenAsync(username, "password123");
        SetAuthToken(token);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("99999"), "SpeciesId"); // Invalid species ID
        form.Add(new StringContent("Test observation"), "Body");
        form.Add(new StringContent(DateTime.UtcNow.ToString("yyyy-MM-dd")), "DateObserved");
        form.Add(new StringContent("52.3676"), "Latitude");
        form.Add(new StringContent("4.9041"), "Longitude");

        // Act
        var response = await _client.PostAsync("/observations", form);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateObservationSimple_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var observationData = new CreateObservationSimpleDTO
        {
            SpeciesId = 1,
            Body = "Test observation",
            DateObserved = DateTime.UtcNow,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var response = await _client.PostAsJsonAsync("/observations/simple", observationData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateObservationSimple_WithValidData_ReturnsOk()
    {
        // Arrange
        var (userId, username, email) = await CreateTestUserAsync();
        var token = await GetAuthTokenAsync(username, "password123");
        var speciesId = await CreateTestSpeciesAsync();
        SetAuthToken(token);

        var observationData = new CreateObservationSimpleDTO
        {
            SpeciesId = speciesId,
            Body = "Test observation",
            DateObserved = DateTime.UtcNow,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var response = await _client.PostAsJsonAsync("/observations/simple", observationData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetObservation_WithValidId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/observations/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var observation = await response.Content.ReadFromJsonAsync<ObservationDTO>();
        Assert.NotNull(observation);
    }

    [Fact]
    public async Task GetObservation_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/observations/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteObservation_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync("/observations/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteObservation_WithValidId_ReturnsOk()
    {
        // Arrange - Create user and observation first
        var (userId, username, email) = await CreateTestUserAsync();
        var token = await GetAuthTokenAsync(username, "password123");
        var speciesId = await CreateTestSpeciesAsync();
        SetAuthToken(token);

        // Create observation
        using var createForm = new MultipartFormDataContent();
        createForm.Add(new StringContent(speciesId.ToString()), "SpeciesId");
        createForm.Add(new StringContent("Test observation"), "Body");
        createForm.Add(new StringContent(DateTime.UtcNow.ToString("yyyy-MM-dd")), "DateObserved");
        createForm.Add(new StringContent("52.3676"), "Latitude");
        createForm.Add(new StringContent("4.9041"), "Longitude");

        var createResponse = await _client.PostAsync("/observations", createForm);
        createResponse.EnsureSuccessStatusCode();

        // Get the created observation ID (this is a simplified approach)
        var observationsResponse = await _client.GetAsync($"/observations/GetAllFromUser/{userId}");
        var observations = await observationsResponse.Content.ReadFromJsonAsync<List<ObservationDTO>>();
        var observationId = observations?.FirstOrDefault()?.Id ?? 1;

        // Act
        var response = await _client.DeleteAsync($"/observations/{observationId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PatchObservation_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var patchData = new PatchObservationDTO
        {
            Body = "Updated observation"
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/observations/1", patchData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateObservationImage_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Array.Empty<byte>());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "image", "test.jpg");

        // Act
        var response = await _client.PatchAsync("/observations/1/image", form);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}