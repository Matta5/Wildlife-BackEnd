using Microsoft.AspNetCore.Mvc;
using Wildlife_BLL.DTO;
using Wildlife_BLL;

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
        public IActionResult CreateUser(CreateEditUserDTO userDTO)
        {
            return Ok(_userService.CreateUser(userDTO));
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            return Ok(_userService.DeleteUser(id));
        }
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, CreateEditUserDTO userDTO)
        {
            return Ok(_userService.UpdateUser(id, userDTO));
        }

    }
}
