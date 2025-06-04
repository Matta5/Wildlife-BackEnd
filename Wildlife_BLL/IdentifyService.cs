using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Wildlife_BLL.DTO;
using Microsoft.Extensions.Configuration;
using Wildlife_BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Wildlife_BLL
{
    public class IdentifyService
    {
        private readonly IIdentifyClient _identifyClient;
        private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public IdentifyService(IIdentifyClient identifyClient)
        {
            _identifyClient = identifyClient;
        }

        public async Task<IdentifyResponseDTO> IdentifyAsync(IdentifyRequestDTO request)
        {
            try
            {
                byte[] imageBytes;

                // Handle file upload
                if (request.ImageFile != null)
                {
                    var validationResult = ValidateFile(request.ImageFile);
                    if (!validationResult.IsValid)
                    {
                        return new IdentifyResponseDTO
                        {
                            Success = false,
                            ErrorMessage = validationResult.ErrorMessage
                        };
                    }

                    using var memoryStream = new MemoryStream();
                    await request.ImageFile.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
                // Handle base64
                else if (!string.IsNullOrEmpty(request.EncodedImage))
                {
                    try
                    {
                        // Remove data URL prefix if present
                        var base64 = request.EncodedImage;
                        if (base64.Contains(','))
                        {
                            base64 = base64.Split(',')[1];
                        }

                        imageBytes = Convert.FromBase64String(base64);

                        if (imageBytes.Length > MaxFileSizeBytes)
                        {
                            return new IdentifyResponseDTO
                            {
                                Success = false,
                                ErrorMessage = "Image too large (max 10MB)"
                            };
                        }
                    }
                    catch (FormatException)
                    {
                        return new IdentifyResponseDTO
                        {
                            Success = false,
                            ErrorMessage = "Invalid base64 image data"
                        };
                    }
                }
                else
                {
                    return new IdentifyResponseDTO
                    {
                        Success = false,
                        ErrorMessage = "No image provided"
                    };
                }

                return await _identifyClient.IdentifyAsync(imageBytes, request.Latitude, request.Longitude);
            }
            catch (Exception ex)
            {
                return new IdentifyResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"Service error: {ex.Message}"
                };
            }
        }

        private (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                return (false, "File is empty");

            if (file.Length > MaxFileSizeBytes)
                return (false, "File too large (max 10MB)");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return (false, "Invalid file type. Allowed: JPG, PNG, WebP");

            return (true, null);
        }
    }
}
