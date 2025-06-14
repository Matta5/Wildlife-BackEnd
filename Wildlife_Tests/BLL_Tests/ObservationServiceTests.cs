using Moq;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace Wildlife_Tests.BLL_Tests;

public class ObservationServiceTests
{
    private readonly Mock<IObservationRepository> _observationRepoMock;
    private readonly Mock<IImageClient> _imageClientMock;
    private readonly ImageService _imageService;
    private readonly ObservationService _observationService;

    public ObservationServiceTests()
    {
        _observationRepoMock = new Mock<IObservationRepository>();
        _imageClientMock = new Mock<IImageClient>();
        _imageService = new ImageService(_imageClientMock.Object);
        _observationService = new ObservationService(_observationRepoMock.Object, _imageService);
    }

    [Fact]
    public async Task CreateObservation_WithImage_UploadsImageAndCreatesObservation()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateObservationDTO
        {
            SpeciesId = 1,
            Body = "Test observation",
            DateObserved = DateTime.UtcNow,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("https://example.com/image.jpg");

        // Act
        await _observationService.CreateObservation(userId, dto, mockFile.Object);

        // Assert
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), "test.jpg"), Times.Once);
        _observationRepoMock.Verify(x => x.CreateObservation(It.Is<CreateObservationDTO>(o =>
            o.UserId == userId &&
            o.SpeciesId == dto.SpeciesId &&
            o.ImageUrl == "https://example.com/image.jpg")), Times.Once);
    }

    [Fact]
    public async Task CreateObservation_WithoutImage_CreatesObservationWithoutImage()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateObservationDTO
        {
            SpeciesId = 1,
            Body = "Test observation",
            DateObserved = DateTime.UtcNow,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        await _observationService.CreateObservation(userId, dto, null);

        // Assert
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _observationRepoMock.Verify(x => x.CreateObservation(It.Is<CreateObservationDTO>(o =>
            o.UserId == userId &&
            o.SpeciesId == dto.SpeciesId &&
            o.ImageUrl == null)), Times.Once);
    }

    [Fact]
    public void CreateObservationSimple_CreatesObservationWithoutImage()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateObservationSimpleDTO
        {
            SpeciesId = 1,
            Body = "Test observation",
            DateObserved = DateTime.UtcNow,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        _observationService.CreateObservationSimple(userId, dto);

        // Assert
        _observationRepoMock.Verify(x => x.CreateObservation(It.Is<CreateObservationDTO>(o =>
            o.UserId == userId &&
            o.SpeciesId == dto.SpeciesId &&
            o.ImageUrl == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateObservationImage_WithValidFile_UpdatesImageAndReturnsUrl()
    {
        // Arrange
        var observationId = 1;
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("new-image.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("https://example.com/new-image.jpg");

        // Act
        var result = await _observationService.UpdateObservationImage(observationId, mockFile.Object);

        // Assert
        Assert.Equal("https://example.com/new-image.jpg", result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), "new-image.jpg"), Times.Once);
        _observationRepoMock.Verify(x => x.PatchObservation(observationId, It.Is<PatchObservationDTO>(p =>
            p.ImageUrl == "https://example.com/new-image.jpg")), Times.Once);
    }

    [Fact]
    public async Task UpdateObservationImage_WithNullFile_ThrowsArgumentException()
    {
        // Arrange
        var observationId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _observationService.UpdateObservationImage(observationId, null));
    }

    [Fact]
    public async Task UpdateObservationImage_WithEmptyFile_ThrowsArgumentException()
    {
        // Arrange
        var observationId = 1;
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _observationService.UpdateObservationImage(observationId, mockFile.Object));
    }

    [Fact]
    public void DeleteObservation_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        var observationId = 1;
        _observationRepoMock.Setup(x => x.DeleteObservation(observationId)).Returns(true);

        // Act
        var result = _observationService.DeleteObservation(observationId);

        // Assert
        Assert.True(result);
        _observationRepoMock.Verify(x => x.DeleteObservation(observationId), Times.Once);
    }

    [Fact]
    public void DeleteObservation_ReturnsFalse_WhenNotDeleted()
    {
        // Arrange
        var observationId = 1;
        _observationRepoMock.Setup(x => x.DeleteObservation(observationId)).Returns(false);

        // Act
        var result = _observationService.DeleteObservation(observationId);

        // Assert
        Assert.False(result);
        _observationRepoMock.Verify(x => x.DeleteObservation(observationId), Times.Once);
    }

    [Fact]
    public void GetObservationById_ReturnsObservation_WhenFound()
    {
        // Arrange
        var observationId = 1;
        var expectedObservation = new ObservationDTO { Id = observationId, Body = "Test" };
        _observationRepoMock.Setup(x => x.GetObservationById(observationId)).Returns(expectedObservation);

        // Act
        var result = _observationService.GetObservationById(observationId);

        // Assert
        Assert.Equal(expectedObservation, result);
        _observationRepoMock.Verify(x => x.GetObservationById(observationId), Times.Once);
    }

    [Fact]
    public void GetObservationById_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var observationId = 1;
        _observationRepoMock.Setup(x => x.GetObservationById(observationId)).Returns((ObservationDTO)null);

        // Act
        var result = _observationService.GetObservationById(observationId);

        // Assert
        Assert.Null(result);
        _observationRepoMock.Verify(x => x.GetObservationById(observationId), Times.Once);
    }

    [Fact]
    public void GetObservationsByUser_ReturnsUserObservations()
    {
        // Arrange
        var userId = 1;
        var expectedObservations = new List<ObservationDTO>
        {
            new ObservationDTO { Id = 1, UserId = userId },
            new ObservationDTO { Id = 2, UserId = userId }
        };
        _observationRepoMock.Setup(x => x.GetObservationsByUser(userId)).Returns(expectedObservations);

        // Act
        var result = _observationService.GetObservationsByUser(userId);

        // Assert
        Assert.Equal(expectedObservations, result);
        _observationRepoMock.Verify(x => x.GetObservationsByUser(userId), Times.Once);
    }

    [Fact]
    public void PatchObservation_ReturnsTrue_WhenUpdated()
    {
        // Arrange
        var observationId = 1;
        var patchDto = new PatchObservationDTO { Body = "Updated observation" };
        _observationRepoMock.Setup(x => x.PatchObservation(observationId, patchDto)).Returns(true);

        // Act
        var result = _observationService.PatchObservation(observationId, patchDto);

        // Assert
        Assert.True(result);
        _observationRepoMock.Verify(x => x.PatchObservation(observationId, patchDto), Times.Once);
    }

    [Fact]
    public void PatchObservation_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        var observationId = 1;
        var patchDto = new PatchObservationDTO { Body = "Updated observation" };
        _observationRepoMock.Setup(x => x.PatchObservation(observationId, patchDto)).Returns(false);

        // Act
        var result = _observationService.PatchObservation(observationId, patchDto);

        // Assert
        Assert.False(result);
        _observationRepoMock.Verify(x => x.PatchObservation(observationId, patchDto), Times.Once);
    }

    [Fact]
    public void GetTotalObservationsByUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = 1;
        var expectedCount = 5;
        _observationRepoMock.Setup(x => x.GetTotalObservationsByUser(userId)).Returns(expectedCount);

        // Act
        var result = _observationService.GetTotalObservationsByUser(userId);

        // Assert
        Assert.Equal(expectedCount, result);
        _observationRepoMock.Verify(x => x.GetTotalObservationsByUser(userId), Times.Once);
    }

    [Fact]
    public void GetUniqueSpeciesCountByUser_ReturnsCorrectCount()
    {
        // Arrange
        var userId = 1;
        var expectedCount = 3;
        _observationRepoMock.Setup(x => x.GetUniqueSpeciesCountByUser(userId)).Returns(expectedCount);

        // Act
        var result = _observationService.GetUniqueSpeciesCountByUser(userId);

        // Assert
        Assert.Equal(expectedCount, result);
        _observationRepoMock.Verify(x => x.GetUniqueSpeciesCountByUser(userId), Times.Once);
    }

    [Fact]
    public void GetAllObservations_ReturnsObservationsWithDefaultParameters()
    {
        // Arrange
        var expectedObservations = new List<ObservationDTO>
        {
            new ObservationDTO { Id = 1 },
            new ObservationDTO { Id = 2 }
        };
        _observationRepoMock.Setup(x => x.GetAllObservations(30, null, false)).Returns(expectedObservations);

        // Act
        var result = _observationService.GetAllObservations();

        // Assert
        Assert.Equal(expectedObservations, result);
        _observationRepoMock.Verify(x => x.GetAllObservations(30, null, false), Times.Once);
    }

    [Fact]
    public void GetAllObservations_ReturnsObservationsWithCustomParameters()
    {
        // Arrange
        var limit = 10;
        var currentUserId = 1;
        var excludeCurrentUser = true;
        var expectedObservations = new List<ObservationDTO>
        {
            new ObservationDTO { Id = 1 }
        };
        _observationRepoMock.Setup(x => x.GetAllObservations(limit, currentUserId, excludeCurrentUser)).Returns(expectedObservations);

        // Act
        var result = _observationService.GetAllObservations(limit, currentUserId, excludeCurrentUser);

        // Assert
        Assert.Equal(expectedObservations, result);
        _observationRepoMock.Verify(x => x.GetAllObservations(limit, currentUserId, excludeCurrentUser), Times.Once);
    }
}