using Moq;
using Wildlife_BLL;
using Wildlife_BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace Wildlife_Tests.BLL_Tests;

public class ImageServiceTests
{
    private readonly Mock<IImageClient> _imageClientMock;
    private readonly ImageService _imageService;

    public ImageServiceTests()
    {
        _imageClientMock = new Mock<IImageClient>();
        _imageService = new ImageService(_imageClientMock.Object);
    }

    [Fact]
    public async Task UploadAsync_WithValidFile_ReturnsImageUrl()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "test-image.jpg";
        var expectedUrl = "https://example.com/uploaded-image.jpg";

        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Equal(expectedUrl, result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WithNullFile_ReturnsNull()
    {
        // Act
        var result = await _imageService.UploadAsync(null);

        // Assert
        Assert.Null(result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_WithEmptyFile_ReturnsNull()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Null(result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_WithLargeFile_UploadsSuccessfully()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "large-image.jpg";
        var expectedUrl = "https://example.com/large-image.jpg";

        mockFile.Setup(f => f.Length).Returns(5 * 1024 * 1024); // 5MB
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[5 * 1024 * 1024]));

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Equal(expectedUrl, result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WithDifferentFileTypes_UploadsSuccessfully()
    {
        // Arrange
        var testCases = new[]
        {
            new { FileName = "image.jpg", ExpectedUrl = "https://example.com/image.jpg" },
            new { FileName = "image.png", ExpectedUrl = "https://example.com/image.png" },
            new { FileName = "image.webp", ExpectedUrl = "https://example.com/image.webp" },
            new { FileName = "image.jpeg", ExpectedUrl = "https://example.com/image.jpeg" }
        };

        foreach (var testCase in testCases)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns(testCase.FileName);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), testCase.FileName))
                .ReturnsAsync(testCase.ExpectedUrl);

            // Act
            var result = await _imageService.UploadAsync(mockFile.Object);

            // Assert
            Assert.Equal(testCase.ExpectedUrl, result);
            _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), testCase.FileName), Times.Once);
        }
    }

    [Fact]
    public async Task UploadAsync_WithSpecialCharactersInFileName_UploadsSuccessfully()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "test-image (1).jpg";
        var expectedUrl = "https://example.com/test-image-1.jpg";

        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Equal(expectedUrl, result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WhenImageClientThrowsException_PropagatesException()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "test-image.jpg";

        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _imageService.UploadAsync(mockFile.Object));

        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WithStreamDisposal_DisposesStreamCorrectly()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "test-image.jpg";
        var expectedUrl = "https://example.com/test-image.jpg";
        var stream = new MemoryStream();

        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Equal(expectedUrl, result);
        // Stream should be disposed after use
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    [Fact]
    public async Task UploadAsync_WithZeroLengthFile_ReturnsNull()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);
        mockFile.Setup(f => f.FileName).Returns("empty.jpg");

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Null(result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_WithNegativeLengthFile_StillUploads()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(-1);
        mockFile.Setup(f => f.FileName).Returns("invalid.jpg");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var expectedUrl = "https://example.com/invalid.jpg";
        _imageClientMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), "invalid.jpg"))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _imageService.UploadAsync(mockFile.Object);

        // Assert
        Assert.Equal(expectedUrl, result);
        _imageClientMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), "invalid.jpg"), Times.Once);
    }
}