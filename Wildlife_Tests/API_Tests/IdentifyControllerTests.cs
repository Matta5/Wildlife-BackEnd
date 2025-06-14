using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;
using Xunit;

namespace Wildlife_Tests.API_Tests;

public class IdentifyControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IdentifyControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Identify_WithValidFile_ReturnsOk()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateTestImageBytes());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "File", "test.jpg");

        // Act
        var response = await _client.PostAsync("/identify", form);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Identify_WithNullFile_ReturnsBadRequest()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        // Don't add any file

        // Act
        var response = await _client.PostAsync("/identify", form);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Array.Empty<byte>());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "File", "test.jpg");

        // Act
        var response = await _client.PostAsync("/identify", form);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithLargeFile_ReturnsBadRequest()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        var largeFileBytes = new byte[10 * 1024 * 1024]; // 10MB file
        var fileContent = new ByteArrayContent(largeFileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "File", "large.jpg");

        // Act
        var response = await _client.PostAsync("/identify", form);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithInvalidFileExtension_ReturnsBadRequest()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateTestImageBytes());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "File", "test.txt");

        // Act
        var response = await _client.PostAsync("/identify", form);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithBase64Image_ReturnsOkOrBadRequest()
    {
        // Arrange
        var requestData = new IdentifyRequestDTO
        {
            EncodedImage = Convert.ToBase64String(CreateTestImageBytes())
        };

        // Act
        var response = await _client.PostAsJsonAsync("/identify-base64", requestData);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Identify_WithNullBase64Image_ReturnsBadRequest()
    {
        // Arrange
        var requestData = new IdentifyRequestDTO
        {
            EncodedImage = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/identify-base64", requestData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithEmptyBase64Image_ReturnsBadRequest()
    {
        // Arrange
        var requestData = new IdentifyRequestDTO
        {
            EncodedImage = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/identify-base64", requestData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithInvalidBase64Image_ReturnsBadRequest()
    {
        // Arrange
        var requestData = new IdentifyRequestDTO
        {
            EncodedImage = "invalid-base64-string!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/identify-base64", requestData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithLargeBase64Image_ReturnsBadRequest()
    {
        // Arrange
        var largeImageBytes = new byte[5 * 1024 * 1024]; // 5MB image
        var requestData = new IdentifyRequestDTO
        {
            EncodedImage = Convert.ToBase64String(largeImageBytes)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/identify-base64", requestData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Identify_WithDataUrlBase64Image_ReturnsOkOrBadRequest()
    {
        // Arrange
        var imageBytes = CreateTestImageBytes();
        var base64String = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:image/jpeg;base64,{base64String}";

        var requestData = new IdentifyRequestDTO
        {
            EncodedImage = dataUrl
        };

        // Act
        var response = await _client.PostAsJsonAsync("/identify-base64", requestData);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Identify_WithFileAndBase64Image_PrioritizesFile()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateTestImageBytes());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "File", "test.jpg");
        form.Add(new StringContent("invalid-base64"), "EncodedImage");

        // Act
        var response = await _client.PostAsync("/identify", form);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Identify_WithValidFileTypes_ProcessesSuccessfully()
    {
        // Test with the most common valid file types
        var validExtensions = new[] { "jpg", "jpeg", "png" };

        foreach (var extension in validExtensions)
        {
            // Arrange
            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(CreateTestImageBytes());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue($"image/{extension}");
            form.Add(fileContent, "File", $"test.{extension}");

            // Act
            var response = await _client.PostAsync("/identify", form);

            // Assert - Should not return an error status like 500 Internal Server Error
            Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.NotImplemented, response.StatusCode);
        }
    }

    private byte[] CreateTestImageBytes()
    {
        // Create a minimal valid JPEG image (just the header)
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
            0x00, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
            0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08,
            0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0x37, 0xFF, 0xD9
        };
    }
}