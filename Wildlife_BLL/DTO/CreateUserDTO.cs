using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.DTO
{
    public class CreateUserDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        public IFormFile? ProfilePicture { get; set; } = null!;
        public string? ProfilePictureURL { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string RefreshToken { get; set; } = "";
        public DateTime RefreshTokenExpiry { get; set; } = DateTime.UtcNow.AddDays(7);
    }

    public class CreateUserSimpleDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;
    }
}
