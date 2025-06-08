using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;
using Xunit;

namespace Wildlife_API_Tests;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CreateTestUserAsync(string username = "testuser", string email = "testuser@example.com")
    {
        // Create multipart form data since your endpoint expects form data
        using var form = new MultipartFormDataContent();

        // Add user data as form fields
        form.Add(new StringContent(username), "Username");
        form.Add(new StringContent(email), "Email");
        form.Add(new StringContent("password123"), "Password");

        // Add empty file content for profile picture (optional)
        var emptyFileContent = new ByteArrayContent(Array.Empty<byte>());
        emptyFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(emptyFileContent, "ProfilePicture", "test.jpg");

        HttpResponseMessage response = await _client.PostAsync("/users", form);
        response.EnsureSuccessStatusCode();

        HttpResponseMessage getAllResponse = await _client.GetAsync("/users");
        if (getAllResponse.IsSuccessStatusCode)
        {
            List<UserDTO>? users = await getAllResponse.Content.ReadFromJsonAsync<List<UserDTO>>();
            UserDTO? createdUser = users?.FirstOrDefault(u => u.Username == username);
            return createdUser?.Id ?? 0;
        }

        throw new InvalidOperationException("Could not retrieve created user ID");
    }

    private MultipartFormDataContent CreateUserFormData(string username, string email, string password, string? fileName = null)
    {
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(username), "Username");
        form.Add(new StringContent(email), "Email");
        form.Add(new StringContent(password), "Password");

        // Add optional profile picture
        if (!string.IsNullOrEmpty(fileName))
        {
            var fileContent = new ByteArrayContent(Array.Empty<byte>());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(fileContent, "ProfilePicture", fileName);
        }

        return form;
    }

    [Fact]
    public async Task GetUserById_ReturnsOk()
    {
        //arrange
        int userId = await CreateTestUserAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound()
    {
        // Arrange
        int nonExistentUserId = 999;
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/users/{nonExistentUserId}");
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsOk()
    {
        // Arrange - Create form data instead of JSON
        using var form = CreateUserFormData("testuser123", "testemail@gmail.com", "password123", "test.jpg");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/users", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithExistingUsername_ReturnsConflict()
    {
        // Arrange - Create first user using form data
        using var firstUserForm = CreateUserFormData("duplicateuser", "first@gmail.com", "password123", "test1.jpg");

        // Act - Create first user (should succeed)
        HttpResponseMessage firstResponse = await _client.PostAsync("/users", firstUserForm);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Arrange - Create second user with same username but different email
        using var secondUserForm = CreateUserFormData("duplicateuser", "second@gmail.com", "password456", "test2.jpg");

        // Act - Try to create second user with duplicate username
        HttpResponseMessage secondResponse = await _client.PostAsync("/users", secondUserForm);

        // Assert - Should return Conflict status
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        string responseContent = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("Username already exists", responseContent);
    }
}