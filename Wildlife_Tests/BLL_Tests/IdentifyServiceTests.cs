using Moq;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace Wildlife_Tests.BLL_Tests;

public class IdentifyServiceTests
{
    private readonly Mock<IIdentifyClient> _identifyClientMock;
    private readonly IdentifyService _identifyService;

    public IdentifyServiceTests()
    {
        _identifyClientMock = new Mock<IIdentifyClient>();
        _identifyService = new IdentifyService(_identifyClientMock.Object);
    }

    [Fact]
    public async Task IdentifyAsync_WithValidFile_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = CreateMockFile("test.jpg", 1024),
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        var expectedResponse = new IdentifyResponseDTO
        {
            Success = true,
            PreferredEnglishName = "Test Species",
            ScientificName = "Testus species",
            Confidence = 0.95,
            AlternativeResults = new List<TaxonResult>
            {
                new TaxonResult { PreferredEnglishName = "Test Species", Confidence = 0.95 }
            }
        };

        _identifyClientMock.Setup(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedResponse.AlternativeResults, result.AlternativeResults);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude), Times.Once);
    }

    [Fact]
    public async Task IdentifyAsync_WithValidBase64Image_ReturnsSuccessResponse()
    {
        // Arrange
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var base64Image = Convert.ToBase64String(imageBytes);
        var request = new IdentifyRequestDTO
        {
            EncodedImage = base64Image,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        var expectedResponse = new IdentifyResponseDTO
        {
            Success = true,
            PreferredEnglishName = "Test Species",
            ScientificName = "Testus species",
            Confidence = 0.95,
            AlternativeResults = new List<TaxonResult>
            {
                new TaxonResult { PreferredEnglishName = "Test Species", Confidence = 0.95 }
            }
        };

        _identifyClientMock.Setup(x => x.IdentifyAsync(imageBytes, request.Latitude, request.Longitude))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedResponse.AlternativeResults, result.AlternativeResults);
        _identifyClientMock.Verify(x => x.IdentifyAsync(imageBytes, request.Latitude, request.Longitude), Times.Once);
    }

    [Fact]
    public async Task IdentifyAsync_WithDataUrlBase64Image_ReturnsSuccessResponse()
    {
        // Arrange
        var imageBytes = new byte[] { 1, 2, 3, 4 };
        var base64Image = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:image/jpeg;base64,{base64Image}";
        var request = new IdentifyRequestDTO
        {
            EncodedImage = dataUrl,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        var expectedResponse = new IdentifyResponseDTO
        {
            Success = true,
            PreferredEnglishName = "Test Species",
            ScientificName = "Testus species",
            Confidence = 0.95,
            AlternativeResults = new List<TaxonResult>
            {
                new TaxonResult { PreferredEnglishName = "Test Species", Confidence = 0.95 }
            }
        };

        _identifyClientMock.Setup(x => x.IdentifyAsync(imageBytes, request.Latitude, request.Longitude))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedResponse.AlternativeResults, result.AlternativeResults);
        _identifyClientMock.Verify(x => x.IdentifyAsync(imageBytes, request.Latitude, request.Longitude), Times.Once);
    }

    [Fact]
    public async Task IdentifyAsync_WithNullFile_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = null,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("No image provided", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithEmptyFile_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = CreateMockFile("test.jpg", 0),
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("File is empty", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithFileTooLarge_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = CreateMockFile("test.jpg", 11 * 1024 * 1024), // 11MB
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("File too large (max 10MB)", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithInvalidFileExtension_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = CreateMockFile("test.txt", 1024),
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid file type. Allowed: JPG, PNG, WebP", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithInvalidBase64_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            EncodedImage = "invalid-base64-data",
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid base64 image data", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithBase64ImageTooLarge_ReturnsErrorResponse()
    {
        // Arrange
        var largeImageBytes = new byte[11 * 1024 * 1024]; // 11MB
        var base64Image = Convert.ToBase64String(largeImageBytes);
        var request = new IdentifyRequestDTO
        {
            EncodedImage = base64Image,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Image too large (max 10MB)", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithEmptyEncodedImage_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            EncodedImage = "",
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("No image provided", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithNullEncodedImage_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            EncodedImage = null,
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("No image provided", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task IdentifyAsync_WithValidFileTypes_ProcessesSuccessfully()
    {
        // Arrange
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        foreach (var extension in validExtensions)
        {
            var request = new IdentifyRequestDTO
            {
                ImageFile = CreateMockFile($"test{extension}", 1024),
                Latitude = 52.3676,
                Longitude = 4.9041
            };

            var expectedResponse = new IdentifyResponseDTO
            {
                Success = true,
                PreferredEnglishName = "Test Species",
                ScientificName = "Testus species",
                Confidence = 0.95,
                AlternativeResults = new List<TaxonResult>
                {
                    new TaxonResult { PreferredEnglishName = "Test Species", Confidence = 0.95 }
                }
            };

            _identifyClientMock.Setup(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _identifyService.IdentifyAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedResponse.AlternativeResults, result.AlternativeResults);
        }
    }

    [Fact]
    public async Task IdentifyAsync_WhenIdentifyClientThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = CreateMockFile("test.jpg", 1024),
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        _identifyClientMock.Setup(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude))
            .ThrowsAsync(new InvalidOperationException("Identification failed"));

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Service error: Identification failed", result.ErrorMessage);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude), Times.Once);
    }

    [Fact]
    public async Task IdentifyAsync_WithFileAndBase64Image_PrioritizesFile()
    {
        // Arrange
        var request = new IdentifyRequestDTO
        {
            ImageFile = CreateMockFile("test.jpg", 1024),
            EncodedImage = "data:image/jpeg;base64,invalid",
            Latitude = 52.3676,
            Longitude = 4.9041
        };

        var expectedResponse = new IdentifyResponseDTO
        {
            Success = true,
            PreferredEnglishName = "Test Species",
            ScientificName = "Testus species",
            Confidence = 0.95,
            AlternativeResults = new List<TaxonResult>
            {
                new TaxonResult { PreferredEnglishName = "Test Species", Confidence = 0.95 }
            }
        };

        _identifyClientMock.Setup(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _identifyService.IdentifyAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedResponse.AlternativeResults, result.AlternativeResults);
        _identifyClientMock.Verify(x => x.IdentifyAsync(It.IsAny<byte[]>(), request.Latitude, request.Longitude), Times.Once);
    }

    private static IFormFile CreateMockFile(string fileName, long length)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[Math.Max(0, (int)length)]));
        return mockFile.Object;
    }
}