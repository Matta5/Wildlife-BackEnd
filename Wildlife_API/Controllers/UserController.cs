using Microsoft.AspNetCore.Mvc;
using Wildlife_BLL.DTO;
using Wildlife_BLL;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Wildlife_API.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            UserDTO? user = _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult CreateUser([FromForm] CreateUserDTO userDTO)
        {
            UserDTO? existingUser = _userService.GetUserByUsername(userDTO.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists");
            }

            UserDTO? existingEmail = _userService.GetUserByEmail(userDTO.Email);
            if (existingEmail != null)
            {
                return Conflict("Email already in use");
            }

            var result = _userService.CreateUser(userDTO, userDTO.ProfilePicture);

            Response.Cookies.Append("token", result.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new{message = "User created successfully"});
        }


        [HttpDelete]
        [Authorize]
        public IActionResult DeleteUser()
        {
            int? userId = GetUserIdFromClaims();

            if (userId is not int id)
            {
                return Unauthorized("You need to log into your account first");
            }

            return Ok(_userService.DeleteUser(id));
        }

        [HttpPatch]
        [Authorize]
        public IActionResult PatchUser([FromForm] PatchUserDTO dto)
        {
            try
            {
                int? userId = GetUserIdFromClaims();
                if (userId == null)
                {
                    return Unauthorized("You need to log into your account firs");
                } int id = userId.Value;

                if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    UserDTO? existingUsername = _userService.GetUserByUsername(dto.Username);
                    if (existingUsername != null && existingUsername.Id != userId)
                    {
                        return Conflict("Username already exists");
                    }
                }

                bool success = _userService.PatchUser(id, dto, dto.ProfilePicture);
                if (!success)
                    return NotFound();

                return Ok(new { messsage = "User patched successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return null;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }



    }
}
