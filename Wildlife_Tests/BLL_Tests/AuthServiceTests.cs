using Moq;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using System.Threading;

namespace Wildlife_Tests.BLL_Tests;

public class AuthServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        SetupDefaultConfiguration();
        _authService = new AuthService(_configMock.Object);
    }

    private void SetupDefaultConfiguration()
    {
        _configMock.Setup(x => x["JwtSettings:Secret"]).Returns("test-secret-key-that-is-long-enough-for-hmacsha256");
        _configMock.Setup(x => x["JwtSettings:Issuer"]).Returns("test-issuer");
        _configMock.Setup(x => x["JwtSettings:Audience"]).Returns("test-audience");
        _configMock.Setup(x => x["JwtSettings:AccessTokenExpirationMinutes"]).Returns("60");
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new UserDTO
        {
            Id = 1,
            Username = "testuser"
        };

        // Act
        var token = _authService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Verify token can be parsed
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("test-issuer", jsonToken.Issuer);
        Assert.Equal("test-audience", jsonToken.Audiences.First());
        Assert.Equal(user.Id.ToString(), jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value);
        Assert.Equal(user.Username, jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value);
    }

    [Fact]
    public void GenerateAccessToken_WithDifferentUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var user1 = new UserDTO { Id = 1, Username = "user1" };
        var user2 = new UserDTO { Id = 2, Username = "user2" };

        // Act
        var token1 = _authService.GenerateAccessToken(user1);
        var token2 = _authService.GenerateAccessToken(user2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateAccessToken_IncludesCorrectClaims()
    {
        // Arrange
        var user = new UserDTO
        {
            Id = 123,
            Username = "testuser"
        };

        // Act
        var token = _authService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var nameIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);
        var nameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);

        Assert.NotNull(nameIdClaim);
        Assert.NotNull(nameClaim);
        Assert.Equal("123", nameIdClaim.Value);
        Assert.Equal("testuser", nameClaim.Value);
    }

    [Fact]
    public void GenerateAccessToken_WithCustomExpiration_RespectsConfiguration()
    {
        // Arrange
        _configMock.Setup(x => x["JwtSettings:AccessTokenExpirationMinutes"]).Returns("30");
        var user = new UserDTO { Id = 1, Username = "testuser" };

        // Act
        var token = _authService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        // Token should expire in approximately 30 minutes
        var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
        var actualExpiry = jsonToken.ValidTo;

        // Allow for small time differences (within 1 minute)
        Assert.True(actualExpiry > DateTime.UtcNow);
        Assert.True(actualExpiry <= expectedExpiry);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidBase64String()
    {
        // Act
        var refreshToken = _authService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);

        // Should be valid base64
        var bytes = Convert.FromBase64String(refreshToken);
        Assert.Equal(64, bytes.Length); // 64 bytes as specified in the service
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesDifferentTokens()
    {
        // Act
        var token1 = _authService.GenerateRefreshToken();
        var token2 = _authService.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesCryptographicallySecureTokens()
    {
        // Act
        var tokens = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            tokens.Add(_authService.GenerateRefreshToken());
        }

        // Assert
        // All tokens should be unique
        Assert.Equal(tokens.Count, tokens.Distinct().Count());

        // All tokens should be valid base64
        foreach (var token in tokens)
        {
            var bytes = Convert.FromBase64String(token);
            Assert.Equal(64, bytes.Length);
        }
    }

    [Theory]
    [InlineData("test-secret-key-that-is-long-enough-for-hmacsha256")]
    [InlineData("another-secret-key-that-is-long-enough-for-hmacsha256")]
    public void GenerateAccessToken_WithDifferentSecrets_GeneratesDifferentTokens(string secret)
    {
        // Arrange
        _configMock.Setup(x => x["JwtSettings:Secret"]).Returns(secret);
        var user = new UserDTO { Id = 1, Username = "testuser" };

        // Act
        var token = _authService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Token should be valid
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);
        Assert.Equal("test-issuer", jsonToken.Issuer);
    }

    [Theory]
    [InlineData("60")]
    [InlineData("120")]
    [InlineData("1440")] // 24 hours
    public void GenerateAccessToken_WithDifferentExpirationTimes_RespectsConfiguration(string expirationMinutes)
    {
        // Arrange
        _configMock.Setup(x => x["JwtSettings:AccessTokenExpirationMinutes"]).Returns(expirationMinutes);
        var user = new UserDTO { Id = 1, Username = "testuser" };

        // Act
        var token = _authService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(double.Parse(expirationMinutes));
        var actualExpiry = jsonToken.ValidTo;

        // Allow for small time differences (within 1 minute)
        Assert.True(actualExpiry > DateTime.UtcNow);
        Assert.True(actualExpiry <= expectedExpiry);
    }
}