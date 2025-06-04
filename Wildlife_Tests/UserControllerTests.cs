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

    [Fact]
    public async Task CreateUser_ReturnsOk()
    {
        CreateUserDTO dto = new CreateUserDTO
        {
            Username = "testuser123",
            Email = "testemail@gmail.com",
            Password = "password"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/users", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithExistingUsername_ReturnsConflict()
    {
        // Arrange - Create first user
        CreateUserDTO firstUserDto = new CreateUserDTO
        {
            Username = "duplicateuser",
            Email = "first@gmail.com",
            Password = "password123"
        };

        // Act - Create first user (should succeed)
        HttpResponseMessage firstResponse = await _client.PostAsJsonAsync("/users", firstUserDto);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Arrange - Create second user with same username but different email
        CreateUserDTO secondUserDto = new CreateUserDTO
        {
            Username = "duplicateuser", // Same username
            Email = "second@gmail.com",
            Password = "password456"
        };

        // Act - Try to create second user with duplicate username
        HttpResponseMessage secondResponse = await _client.PostAsJsonAsync("/users", secondUserDto);

        // Assert - Should return Conflict status
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        // Optionally, you can also verify the response content
        string responseContent = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("Username already exists", responseContent);
    }
}