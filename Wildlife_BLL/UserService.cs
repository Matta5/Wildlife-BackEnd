using System.Threading.Tasks;
using Wildlife_BLL.Interfaces;
using Wildlife_BLL.DTO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Wildlife_BLL
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public List<UserDTO> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }
        public UserDTO? GetUserById(int id)
        {
            return _userRepository.GetUserById(id);
        }
        public bool CreateUser(CreateEditUserDTO userDTO)
        {
            var passwordHasher = new PasswordHasher<object>();
            userDTO.Password = passwordHasher.HashPassword(null, userDTO.Password);

            return _userRepository.CreateUser(userDTO);
        }

        public bool DeleteUser(int id)
        {
            return _userRepository.DeleteUser(id);
        }
        public bool UpdateUser(int id, CreateEditUserDTO userDTO)
        {
            var passwordHasher = new PasswordHasher<object>();
            userDTO.Password = passwordHasher.HashPassword(null, userDTO.Password);

            return _userRepository.UpdateUser(id, userDTO);
        }

        public UserDTO? GetUserByUsername(string username)
        {
            return _userRepository.GetUserByUsername(username);
        }

        public UserDTO? GetUserByRefreshToken(string refreshToken)
        {
            return _userRepository.GetUserByRefreshToken(refreshToken);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var passwordHasher = new PasswordHasher<object>();

            var result = passwordHasher.VerifyHashedPassword(null, passwordHash, password);

            return result == PasswordVerificationResult.Success;
        }

        public void UpdateRefreshToken(int userId, string refreshToken, DateTime expiry)
        {
            UserDTO user = _userRepository.GetUserById(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            CreateEditUserDTO createEditUserDTO = new CreateEditUserDTO
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.PasswordHash,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt,
                RefreshToken = refreshToken, // Assign new refresh token
                RefreshTokenExpiry = expiry  // Assign new expiry date
            };

            _userRepository.UpdateUser(user.Id, createEditUserDTO); // Assuming this method commits the changes to the database
        }

    }
}
