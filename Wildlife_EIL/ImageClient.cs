using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using Wildlife_BLL.Interfaces;

public class ImageClient : IImageClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ImageClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Imgbb:ApiKey"] ?? throw new Exception("Imgbb API key is not configured.");
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        using MultipartFormDataContent content = new MultipartFormDataContent();

        content.Add(new StringContent(_apiKey), "key");

        StreamContent imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", fileName);

        var response = await _httpClient.PostAsync("https://api.imgbb.com/1/upload", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Image upload failed: " + await response.Content.ReadAsStringAsync());

        string json = await response.Content.ReadAsStringAsync();

        using JsonDocument doc = JsonDocument.Parse(json);
        string? url = doc.RootElement.GetProperty("data").GetProperty("url").GetString();

        return url!;
    }
}
