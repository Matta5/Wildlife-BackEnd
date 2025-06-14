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
    private readonly ImageService _imageService;
    private readonly ObservationService _observationService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _imageClientMock = new Mock<IImageClient>();
        _observationRepoMock = new Mock<IObservationRepository>();
        _imageService = new ImageService(_imageClientMock.Object);
        _observationService = new ObservationService(_observationRepoMock.Object, _imageService);
        _userService = new UserService(_userRepoMock.Object, _authServiceMock.Object, _imageService, _observationService);
    }

    [Fact]
    public void GetAllUsers_ReturnsAllUsers()
    {
        // Arrange
        var expectedUsers = new List<UserDTO> { new UserDTO { Id = 1, Username = "test" } };
        _userRepoMock.Setup(r => r.GetAllUsers()).Returns(expectedUsers);
        _observationRepoMock.Setup(r => r.GetTotalObservationsByUser(1)).Returns(5);
        _observationRepoMock.Setup(r => r.GetUniqueSpeciesCountByUser(1)).Returns(3);

        // Act
        var result = _userService.GetAllUsers();

        // Assert
        Assert.Equal(expectedUsers, result);
    }

    [Fact]
    public void GetUserById_ReturnsUser_WhenFound()
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
    }

    [Fact]
    public void CreateUser_CreatesUser_WhenUsernameIsUnique()
    {
        // Arrange
        var newUser = new CreateUserDTO { Username = "uniqueUser", Password = "pass" };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _userRepoMock.Setup(r => r.GetUserByUsername("uniqueuser")).Returns((UserDTO?)null);
        _userRepoMock.Setup(r => r.CreateUser(It.IsAny<CreateUserDTO>())).Returns(true);
        _userRepoMock.Setup(r => r.GetUserByUsername("uniqueUser")).Returns(new UserDTO { Id = 1, Username = "uniqueUser" });
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/image.jpg");
        _authServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _authServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserDTO>())).Returns("access-token");

        // Act
        var result = _userService.CreateUser(newUser, mockFile.Object);
        // Assert

        Assert.True(result != null);
        _userRepoMock.Verify(r => r.CreateUser(It.Is<CreateUserDTO>(u => u.Username == "uniqueUser")), Times.Once);
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
    public void PatchUser_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var userId = 1;
        var updateDto = new PatchUserDTO { Username = "updated", Password = "newpass" };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024L);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _userRepoMock.Setup(r => r.GetUserById(userId)).Returns(new UserDTO { Id = userId });
        _userRepoMock
            .Setup(r => r.PatchUser(userId, It.IsAny<PatchUserDTO>()))
            .Returns(true);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/image.jpg");

        // Act
        var result = _userService.PatchUser(userId, updateDto, mockFile.Object);

        // Assert
        Assert.True(result);
        _userRepoMock.Verify(r => r.PatchUser(userId, It.Is<PatchUserDTO>(
            dto => !string.IsNullOrWhiteSpace(dto.Password) && dto.Username == "updated"
        )), Times.Once);
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
        _userRepoMock
            .Setup(r => r.PatchUser(userId, It.IsAny<PatchUserDTO>()))
            .Returns(false);
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>())).ReturnsAsync("https://example.com/image.jpg");

        // Act
        var result = _userService.PatchUser(userId, updateDto, mockFile.Object);

        // Assert
        Assert.False(result);
        _userRepoMock.Verify(r => r.PatchUser(userId, It.Is<PatchUserDTO>(
            dto => !string.IsNullOrWhiteSpace(dto.Password) && dto.Username == "updated"
        )), Times.Once);
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
    public void UpdateRefreshToken_ValidUser_UpdatesToken()
    {
        // Arrange
        int userId = 1;
        string token = "new-token";
        DateTime expiry = DateTime.UtcNow.AddDays(7);
        _userRepoMock.Setup(r => r.GetUserById(userId)).Returns(new UserDTO { Id = userId });

        // Act
        _userService.UpdateRefreshToken(userId, token, expiry);

        // Assert
        _userRepoMock.Verify(r => r.PatchUser(userId, It.Is<PatchUserDTO>(dto =>
            dto.RefreshToken == token && dto.RefreshTokenExpiry == expiry
        )), Times.Once);
    }
}
