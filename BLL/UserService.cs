using System.Threading.Tasks;
using Wildlife_BLL.Interfaces;
using Wildlife_BLL.DTO;

namespace Wildlife_BLL
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public int? CreateUserAsync(CreateEditUserDTO user)
        {
            if (_userRepository.GetByEmailOrUsernameAsync(user.Email, user.Username) != null)
                return null; // User already exists
            try
            {
                return _userRepository.CreateUserAsync(user);
            }
            catch
            {
                return null; // Error creating user
            }
        }
    }
}
