using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;
using Xunit;

namespace Wildlife_Tests.API_Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string Username, string Email)> CreateTestUserAsync()
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

        return (username, email);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var (username, email) = await CreateTestUserAsync();
        var loginData = new LoginDTO
        {
            Username = username,
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);

        // Check that cookies are set
        Assert.True(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var loginData = new LoginDTO
        {
            Username = "nonexistentuser",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", content);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var (username, email) = await CreateTestUserAsync();
        var loginData = new LoginDTO
        {
            Username = username,
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", content);
    }

    [Fact]
    public async Task Login_WithCaseInsensitiveUsername_ReturnsOk()
    {
        // Arrange
        var (username, email) = await CreateTestUserAsync();
        var loginData = new LoginDTO
        {
            Username = username.ToUpper(), // Test case insensitivity
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithoutRefreshToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsync("/auth/refresh", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Refresh token not found", content);
    }

    private void SetCookie(string name, string value)
    {
        _client.DefaultRequestHeaders.Remove("Cookie");
        _client.DefaultRequestHeaders.Add("Cookie", $"{name}={value}");
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOk()
    {
        // Arrange - Login to get refresh token
        var (username, email) = await CreateTestUserAsync();
        var loginData = new LoginDTO
        {
            Username = username,
            Password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginData);
        loginResponse.EnsureSuccessStatusCode();

        // Extract refresh token from cookies
        var refreshToken = ExtractRefreshTokenFromCookies(loginResponse);
        SetCookie("refreshToken", refreshToken);

        // Act
        var response = await _client.PostAsync("/auth/refresh", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ReturnsOk()
    {
        // Act
        var response = await _client.PostAsync("/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Logout successful", content);
    }

    [Fact]
    public async Task Logout_WithValidToken_ReturnsOk()
    {
        // Arrange - Login to get refresh token
        var (username, email) = await CreateTestUserAsync();
        var loginData = new LoginDTO
        {
            Username = username,
            Password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginData);
        loginResponse.EnsureSuccessStatusCode();

        // Extract refresh token from cookies
        var refreshToken = ExtractRefreshTokenFromCookies(loginResponse);
        SetCookie("refreshToken", refreshToken);

        // Act
        var response = await _client.PostAsync("/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsOk()
    {
        // Arrange - Login to get access token
        var (username, email) = await CreateTestUserAsync();
        var loginData = new LoginDTO
        {
            Username = username,
            Password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginData);
        loginResponse.EnsureSuccessStatusCode();

        // Extract access token from Authorization header
        var authHeader = loginResponse.Headers.GetValues("Authorization").FirstOrDefault();
        var accessToken = authHeader?.Replace("Bearer ", "");
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        // Act
        var response = await _client.GetAsync("/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserDTO>();
        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
        Assert.Equal(email, user.Email);
    }

    private string? ExtractRefreshTokenFromCookies(HttpResponseMessage response)
    {
        var setCookieHeaders = response.Headers.GetValues("Set-Cookie");
        foreach (var cookie in setCookieHeaders)
        {
            if (cookie.Contains("refreshToken="))
            {
                var startIndex = cookie.IndexOf("refreshToken=") + "refreshToken=".Length;
                var endIndex = cookie.IndexOf(';', startIndex);
                if (endIndex == -1) endIndex = cookie.Length;
                return cookie.Substring(startIndex, endIndex - startIndex);
            }
        }
        return null;
    }
}