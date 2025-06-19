using Moq;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Wildlife_Tests.BLL_Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IImageClient> _imageClientMock;
    private readonly Mock<IObservationRepository> _observationRepoMock;
    private readonly Mock<IObservationNotificationService> _notificationServiceMock;
    private readonly ImageService _imageService;
    private readonly ObservationService _observationService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _imageClientMock = new Mock<IImageClient>();
        _observationRepoMock = new Mock<IObservationRepository>();
        _notificationServiceMock = new Mock<IObservationNotificationService>();
        _imageService = new ImageService(_imageClientMock.Object);
        _observationService = new ObservationService(_observationRepoMock.Object, _imageService, _notificationServiceMock.Object);
        _userService = new UserService(_userRepoMock.Object, _authServiceMock.Object, _imageService, _observationService);
    }

    [Fact]
    public void GetAllUsers_ReturnsAllUsersWithStatistics()
    {
        // Arrange
        var expectedUsers = new List<UserDTO> 
        { 
            new UserDTO { Id = 1, Username = "test1" },
            new UserDTO { Id = 2, Username = "test2" }
        };
        _userRepoMock.Setup(r => r.GetAllUsers()).Returns(expectedUsers);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(1)).Returns(5);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(1)).Returns(3);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(2)).Returns(10);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(2)).Returns(7);

        // Act
        var result = _userService.GetAllUsers();

        // Assert
        Assert.Equal(expectedUsers, result);
        Assert.Equal(5, result[0].TotalObservations);
        Assert.Equal(3, result[0].UniqueSpeciesObserved);
        Assert.Equal(10, result[1].TotalObservations);
        Assert.Equal(7, result[1].UniqueSpeciesObserved);
        _userRepoMock.Verify(r => r.GetAllUsers(), Times.Once);
    }

    [Fact]
    public void GetAllUsers_ReturnsEmptyList_WhenNoUsers()
    {
        // Arrange
        var expectedUsers = new List<UserDTO>();
        _userRepoMock.Setup(r => r.GetAllUsers()).Returns(expectedUsers);

        // Act
        var result = _userService.GetAllUsers();

        // Assert
        Assert.Empty(result);
        _userRepoMock.Verify(r => r.GetAllUsers(), Times.Once);
    }

    [Fact]
    public void GetUserById_ReturnsUserWithStatistics_WhenFound()
    {
        // Arrange
        var expectedUser = new UserDTO { Id = 1, Username = "test" };
        _userRepoMock.Setup(r => r.GetUserById(1)).Returns(expectedUser);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(1)).Returns(5);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(1)).Returns(3);

        // Act
        var result = _userService.GetUserById(1);

        // Assert
        Assert.Equal(expectedUser, result);
        Assert.Equal(5, result.TotalObservations);
        Assert.Equal(3, result.UniqueSpeciesObserved);
        _userRepoMock.Verify(r => r.GetUserById(1), Times.Once);
    }

    [Fact]
    public void GetUserById_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetUserById(1)).Returns((UserDTO?)null);
        
        // Act
        var result = _userService.GetUserById(1);
        
        // Assert
        Assert.Null(result);
        _userRepoMock.Verify(r => r.GetUserById(1), Times.Once);
        _observationRepoMock.Verify(r => r.GetTotalObservationsByUser(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CreateUser_CreatesUserWithProfilePicture_WhenValidData()
    {
        // Arrange
        var newUser = new CreateUserDTO { Username = "uniqueUser", Password = "pass", Email = "test@example.com" };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var createdUser = new UserDTO { Id = 1, Username = "uniqueUser", Email = "test@example.com" };

        _userRepoMock.Setup(r => r.CreateUser(It.IsAny<CreateUserDTO>())).Returns(true);
        _userRepoMock.Setup(r => r.GetUserByUsername("uniqueUser")).Returns(createdUser);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/image.jpg");
        _authServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _authServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserDTO>())).Returns("access-token");

        // Act
        var result = _userService.CreateUser(newUser, mockFile.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        _userRepoMock.Verify(r => r.CreateUser(It.Is<CreateUserDTO>(u => 
            u.Username == "uniqueUser" && 
            u.Password != "pass" && // Should be hashed
            u.ProfilePictureURL == "https://example.com/image.jpg" &&
            u.RefreshToken == "refresh-token")), Times.Once);
        _authServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _authServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserDTO>()), Times.Once);
    }

    [Fact]
    public void CreateUser_CreatesUserWithNullProfilePicture_WhenNoFile()
    {
        // Arrange
        var newUser = new CreateUserDTO { Username = "uniqueUser", Password = "pass", Email = "test@example.com" };
        var createdUser = new UserDTO { Id = 1, Username = "uniqueUser", Email = "test@example.com" };

        _userRepoMock.Setup(r => r.CreateUser(It.IsAny<CreateUserDTO>())).Returns(true);
        _userRepoMock.Setup(r => r.GetUserByUsername("uniqueUser")).Returns(createdUser);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync((string?)null);
        _authServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _authServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserDTO>())).Returns("access-token");

        // Act
        var result = _userService.CreateUser(newUser, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        _userRepoMock.Verify(r => r.CreateUser(It.Is<CreateUserDTO>(u => 
            u.Username == "uniqueUser" && 
            u.Password != "pass" && // Should be hashed
            u.ProfilePictureURL == null)), Times.Once);
    }

    [Fact]
    public void CreateUserSimple_CreatesUserWithoutProfilePicture()
    {
        // Arrange
        var newUser = new CreateUserSimpleDTO { Username = "simpleUser", Password = "pass", Email = "simple@example.com" };
        var createdUser = new UserDTO { Id = 1, Username = "simpleUser", Email = "simple@example.com" };

        _userRepoMock.Setup(r => r.CreateUser(It.IsAny<CreateUserDTO>())).Returns(true);
        _userRepoMock.Setup(r => r.GetUserByUsername("simpleUser")).Returns(createdUser);
        _authServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _authServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserDTO>())).Returns("access-token");

        // Act
        var result = _userService.CreateUserSimple(newUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        _userRepoMock.Verify(r => r.CreateUser(It.Is<CreateUserDTO>(u => 
            u.Username == "simpleUser" && 
            u.Email == "simple@example.com" &&
            u.Password != "pass" && // Should be hashed
            u.ProfilePictureURL == null &&
            u.RefreshToken == "refresh-token")), Times.Once);
        _authServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _authServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserDTO>()), Times.Once);
    }

    [Fact]
    public void PatchUser_ReturnsTrue_WhenUpdateSucceedsWithPassword()
    {
        // Arrange
        var userId = 1;
        var updateDto = new PatchUserDTO { Username = "updated", Password = "newpass" };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _userRepoMock.Setup(r => r.GetUserById(userId)).Returns(new UserDTO { Id = userId });
        _userRepoMock.Setup(r => r.PatchUser(userId, It.IsAny<PatchUserDTO>())).Returns(true);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/new-image.jpg");

        // Act
        var result = _userService.PatchUser(userId, updateDto, mockFile.Object);

        // Assert
        Assert.True(result);
        _userRepoMock.Verify(r => r.PatchUser(userId, It.Is<PatchUserDTO>(dto => 
            dto.Username == "updated" && 
            !string.IsNullOrWhiteSpace(dto.Password) && 
            dto.Password != "newpass" && // Should be hashed
            dto.ProfilePictureURL == "https://example.com/new-image.jpg")), Times.Once);
    }

    [Fact]
    public void PatchUser_ReturnsTrue_WhenUpdateSucceedsWithoutPassword()
    {
        // Arrange
        var userId = 1;
        var updateDto = new PatchUserDTO { Username = "updated" }; // No password
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _userRepoMock.Setup(r => r.GetUserById(userId)).Returns(new UserDTO { Id = userId });
        _userRepoMock.Setup(r => r.PatchUser(userId, It.IsAny<PatchUserDTO>())).Returns(true);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/new-image.jpg");

        // Act
        var result = _userService.PatchUser(userId, updateDto, mockFile.Object);

        // Assert
        Assert.True(result);
        _userRepoMock.Verify(r => r.PatchUser(userId, It.Is<PatchUserDTO>(dto => 
            dto.Username == "updated" && 
            string.IsNullOrWhiteSpace(dto.Password) && 
            dto.ProfilePictureURL == "https://example.com/new-image.jpg")), Times.Once);
    }

    [Fact]
    public void PatchUser_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        var userId = 999;
        var updateDto = new PatchUserDTO { Username = "updated", Password = "newpass" };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _userRepoMock.Setup(r => r.GetUserById(userId)).Returns((UserDTO?)null);

        // Act
        var result = _userService.PatchUser(userId, updateDto, mockFile.Object);

        // Assert
        Assert.False(result);
        _userRepoMock.Verify(r => r.GetUserById(userId), Times.Once);
        _userRepoMock.Verify(r => r.PatchUser(It.IsAny<int>(), It.IsAny<PatchUserDTO>()), Times.Never);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PatchUser_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        var userId = 2;
        var updateDto = new PatchUserDTO { Username = "updated", Password = "newpass" };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _userRepoMock.Setup(r => r.GetUserById(userId)).Returns(new UserDTO { Id = userId });
        _userRepoMock.Setup(r => r.PatchUser(userId, It.IsAny<PatchUserDTO>())).Returns(false);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/image.jpg");

        // Act
        var result = _userService.PatchUser(userId, updateDto, mockFile.Object);

        // Assert
        Assert.False(result);
        _userRepoMock.Verify(r => r.PatchUser(userId, It.IsAny<PatchUserDTO>()), Times.Once);
    }

    [Fact]
    public void GetUserByUsername_ReturnsUserWithStatistics_WhenFound()
    {
        // Arrange
        var expectedUser = new UserDTO { Id = 1, Username = "testuser" };
        _userRepoMock.Setup(r => r.GetUserByUsername("testuser")).Returns(expectedUser);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(1)).Returns(5);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(1)).Returns(3);

        // Act
        var result = _userService.GetUserByUsername("TestUser"); // Should be converted to lowercase

        // Assert
        Assert.Equal(expectedUser, result);
        Assert.Equal(5, result.TotalObservations);
        Assert.Equal(3, result.UniqueSpeciesObserved);
        _userRepoMock.Verify(r => r.GetUserByUsername("testuser"), Times.Once);
    }

    [Fact]
    public void GetUserByUsername_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetUserByUsername("nonexistent")).Returns((UserDTO?)null);

        // Act
        var result = _userService.GetUserByUsername("NonExistent");

        // Assert
        Assert.Null(result);
        _userRepoMock.Verify(r => r.GetUserByUsername("nonexistent"), Times.Once);
        _observationRepoMock.Verify(r => r.GetTotalObservationsByUser(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetUserByRefreshToken_ReturnsUserWithStatistics_WhenFound()
    {
        // Arrange
        var expectedUser = new UserDTO { Id = 1, Username = "testuser" };
        _userRepoMock.Setup(r => r.GetUserByRefreshToken("valid-token")).Returns(expectedUser);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(1)).Returns(5);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(1)).Returns(3);

        // Act
        var result = _userService.GetUserByRefreshToken("valid-token");

        // Assert
        Assert.Equal(expectedUser, result);
        Assert.Equal(5, result.TotalObservations);
        Assert.Equal(3, result.UniqueSpeciesObserved);
        _userRepoMock.Verify(r => r.GetUserByRefreshToken("valid-token"), Times.Once);
    }

    [Fact]
    public void GetUserByRefreshToken_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetUserByRefreshToken("invalid-token")).Returns((UserDTO?)null);

        // Act
        var result = _userService.GetUserByRefreshToken("invalid-token");

        // Assert
        Assert.Null(result);
        _userRepoMock.Verify(r => r.GetUserByRefreshToken("invalid-token"), Times.Once);
        _observationRepoMock.Verify(r => r.GetTotalObservationsByUser(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetUserByEmail_ReturnsUserWithStatistics_WhenFound()
    {
        // Arrange
        var expectedUser = new UserDTO { Id = 1, Username = "testuser", Email = "test@example.com" };
        _userRepoMock.Setup(r => r.GetUserByEmail("test@example.com")).Returns(expectedUser);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(1)).Returns(5);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(1)).Returns(3);

        // Act
        var result = _userService.GetUserByEmail("Test@Example.com"); // Should be converted to lowercase

        // Assert
        Assert.Equal(expectedUser, result);
        Assert.Equal(5, result.TotalObservations);
        Assert.Equal(3, result.UniqueSpeciesObserved);
        _userRepoMock.Verify(r => r.GetUserByEmail("test@example.com"), Times.Once);
    }

    [Fact]
    public void GetUserByEmail_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetUserByEmail("nonexistent@example.com")).Returns((UserDTO?)null);

        // Act
        var result = _userService.GetUserByEmail("NonExistent@Example.com");

        // Assert
        Assert.Null(result);
        _userRepoMock.Verify(r => r.GetUserByEmail("nonexistent@example.com"), Times.Once);
        _observationRepoMock.Verify(r => r.GetTotalObservationsByUser(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForValidPassword()
    {
        // Arrange
        var password = "securePassword";
        var hasher = new PasswordHasher<object>();
        var hashed = hasher.HashPassword(null, password);

        // Act
        var result = _userService.VerifyPassword(password, hashed);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForInvalidPassword()
    {
        // Arrange
        var password = "securePassword";
        var wrongPassword = "wrongPassword";
        var hasher = new PasswordHasher<object>();
        var hashed = hasher.HashPassword(null, password);

        // Act
        var result = _userService.VerifyPassword(wrongPassword, hashed);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForEmptyPassword()
    {
        // Arrange
        var password = "";
        var hasher = new PasswordHasher<object>();
        var hashed = hasher.HashPassword(null, "somePassword");

        // Act
        var result = _userService.VerifyPassword(password, hashed);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateRefreshToken_UpdatesUserWithNewToken()
    {
        // Arrange
        int userId = 1;
        string token = "new-refresh-token";
        DateTime expiry = DateTime.UtcNow.AddDays(7);

        // Act
        _userService.UpdateRefreshToken(userId, token, expiry);

        // Assert
        _userRepoMock.Verify(r => r.PatchUser(userId, It.Is<PatchUserDTO>(dto =>
            dto.RefreshToken == token && 
            dto.RefreshTokenExpiry == expiry)), Times.Once);
    }

    [Fact]
    public void DeleteUser_ReturnsTrue_WhenUserDeleted()
    {
        // Arrange
        int userId = 2;
        _userRepoMock.Setup(r => r.DeleteUser(userId)).Returns(true);

        // Act
        var result = _userService.DeleteUser(userId);

        // Assert
        Assert.True(result);
        _userRepoMock.Verify(r => r.DeleteUser(userId), Times.Once);
    }

    [Fact]
    public void DeleteUser_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        int userId = 3;
        _userRepoMock.Setup(r => r.DeleteUser(userId)).Returns(false);
        
        // Act
        var result = _userService.DeleteUser(userId);
        
        // Assert
        Assert.False(result);
        _userRepoMock.Verify(r => r.DeleteUser(userId), Times.Once);
    }

    [Fact]
    public void DeleteUser_ReturnsFalse_WhenDeleteFails()
    {
        // Arrange
        int userId = 4;
        _userRepoMock.Setup(r => r.DeleteUser(userId)).Returns(false);
        
        // Act
        var result = _userService.DeleteUser(userId);
        
        // Assert
        Assert.False(result);
        _userRepoMock.Verify(r => r.DeleteUser(userId), Times.Once);
    }
}
