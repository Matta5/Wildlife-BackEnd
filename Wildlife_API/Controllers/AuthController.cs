using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;

namespace Wildlife_BackEnd.Controllers;


[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IAuthService _authService;

    public AuthController(UserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO loginDto)
    {
        // Normalize username to lowercase for comparison
        UserDTO? user = _userService.GetUserByUsername(loginDto.Username.ToLower());
        if (user == null || !_userService.VerifyPassword(loginDto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var accessToken = _authService.GenerateAccessToken(user);
        var refreshToken = _authService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        _userService.UpdateRefreshToken(user.Id, refreshToken, refreshTokenExpiry);

        var authenticatedUserDto = new AuthenticatedUserDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePicture = user.ProfilePicture,
            CreatedAt = user.CreatedAt
        };

        Response.Headers.Append("Authorization", $"Bearer {accessToken}");

        Response.Cookies.Append("token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true
        });

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true
        });

        return Ok(new { message = "Login successful", user = authenticatedUserDto });
    }




    [HttpPost("refresh")]
    public IActionResult RefreshToken()
    {
        // Lees de refresh token uit de cookie in plaats van request body
        if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Refresh token not found");
        }

        UserDTO? user = _userService.GetUserByRefreshToken(refreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized("Invalid refresh token");


        var newAccessToken = _authService.GenerateAccessToken(user);
        var newRefreshToken = _authService.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update refresh token
        _userService.UpdateRefreshToken(user.Id, newRefreshToken, newRefreshTokenExpiry);

        // Set new JWT in HttpOnly cookie
        Response.Cookies.Append("token", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        // Set new refresh token in HttpOnly cookie
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
            });

        // Stuur alleen een bevestiging terug, geen tokens
        return Ok(new { message = "Token refreshed successfully" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("No refresh token found");

        UserDTO? user = _userService.GetUserByRefreshToken(refreshToken);
        if (user == null)
            return Unauthorized("Invalid refresh token");

        _userService.UpdateRefreshToken(user.Id, refreshToken, DateTime.UtcNow);

        Response.Cookies.Delete("token");
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logout successful" });
    }

    
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("Invalid or no token");

        int userId = int.Parse(userIdClaim.Value);

        UserDTO? user = _userService.GetUserById(userId);
        if (user == null)
            return NotFound("User not found");

        AuthenticatedUserDTO authenticatedUserDto = new AuthenticatedUserDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePicture = user.ProfilePicture,
            CreatedAt = user.CreatedAt,
            TotalObservations = user.TotalObservations,
            UniqueSpeciesObserved = user.UniqueSpeciesObserved
        };
        return Ok(authenticatedUserDto);
    }



}

