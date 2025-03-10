using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WildlifeBLL.DTO;
using WildlifeBLL.Services;

namespace YourNamespace.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var createdUser = await _userService.CreateUserAsync(user);
            if (createdUser == null)
                return BadRequest("User already exists.");

            return Ok(createdUser);
        }
    }
}
