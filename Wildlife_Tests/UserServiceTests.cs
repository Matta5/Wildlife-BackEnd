using Xunit;
using Moq;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Identity;

namespace Wildlife_BLL_Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _userService = new UserService(_userRepoMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public void GetAllUsers_ReturnsAllUsers()
    {
        // Arrange
        var expectedUsers = new List<UserDTO> { new UserDTO { Id = 1, Username = "test" } };
        _userRepoMock.Setup(r => r.GetAllUsers()).Returns(expectedUsers);

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

        // Act
        var result = _userService.GetUserById(1);

        // Assert
        Assert.Equal(expectedUser, result);
    }

    [Fact]
    public void GetUserById_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetUserById(1)).Returns((UserDTO)null);
        // Act
        var result = _userService.GetUserById(1);
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CreateUser_ThrowsException_WhenUsernameExists()
    {
        // Arrange
        var newUser = new CreateEditUserDTO { Username = "TestUser", Password = "pass" };
        _userRepoMock.Setup(r => r.GetUserByUsername("testuser")).Returns(new UserDTO());

        // Act & Assert
        Assert.Throws<Exception>(() => _userService.CreateUser(newUser));
    }

    [Fact]
    public void CreateUser_CreatesUser_WhenUsernameIsUnique()
    {
        // Arrange
        var newUser = new CreateEditUserDTO { Username = "uniqueUser", Password = "pass" };
        _userRepoMock.Setup(r => r.GetUserByUsername("uniqueuser")).Returns((UserDTO)null);
        _userRepoMock.Setup(r => r.CreateUser(It.IsAny<CreateEditUserDTO>())).Returns(true);
        // Act
        var result = _userService.CreateUser(newUser);
        // Assert

        Assert.True(result != null);
        _userRepoMock.Verify(r => r.CreateUser(It.Is<CreateEditUserDTO>(u => u.Username == "uniqueUser")), Times.Once);
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
    public void UpdateUser_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var userId = 1;
        var updateDto = new CreateEditUserDTO { Username = "updated", Password = "newpass" };

        _userRepoMock
            .Setup(r => r.UpdateUser(userId, It.IsAny<CreateEditUserDTO>()))
            .Returns(true);

        // Act
        var result = _userService.UpdateUser(userId, updateDto);

        // Assert
        Assert.True(result);
        _userRepoMock.Verify(r => r.UpdateUser(userId, It.Is<CreateEditUserDTO>(
            dto => !string.IsNullOrWhiteSpace(dto.Password) && dto.Username == "updated"
        )), Times.Once);
    }

    [Fact]
    public void UpdateUser_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        var userId = 2;
        var updateDto = new CreateEditUserDTO { Username = "updated", Password = "newpass" };
        _userRepoMock
            .Setup(r => r.UpdateUser(userId, It.IsAny<CreateEditUserDTO>()))
            .Returns(false);
        // Act
        var result = _userService.UpdateUser(userId, updateDto);
        // Assert
        Assert.False(result);
        _userRepoMock.Verify(r => r.UpdateUser(userId, It.Is<CreateEditUserDTO>(
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
    public void test()
    {
        bool result = true;

        Assert.True(result);
    }

}
