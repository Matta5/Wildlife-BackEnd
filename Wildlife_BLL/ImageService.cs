using Microsoft.AspNetCore.Http;
using Wildlife_BLL.Interfaces;

namespace Wildlife_BLL;
public class ImageService
{
    private readonly IImageClient _imageClient;

    public ImageService(IImageClient imageClient)
    {
        _imageClient = imageClient;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        using Stream stream = file.OpenReadStream();
        return await _imageClient.UploadImageAsync(stream, file.FileName);
    }
}
