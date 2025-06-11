using System.Threading.Tasks;
using Wildlife_BLL.Interfaces;
using Wildlife_BLL.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;

namespace Wildlife_BLL
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ImageService _imageService;
        private readonly ObservationService _observationService;

        public UserService(IUserRepository userRepository, IAuthService authService, ImageService imageService, ObservationService observationService)
        {
            _userRepository = userRepository;
            _authService = authService;
            _imageService = imageService;
            _observationService = observationService;
        }
        public List<UserDTO> GetAllUsers()
        {
            var users = _userRepository.GetAllUsers();
            
            // Populate statistics for each user
            foreach (var user in users)
            {
                user.TotalObservations = _observationService.GetTotalObservationsByUser(user.Id);
                user.UniqueSpeciesObserved = _observationService.GetUniqueSpeciesCountByUser(user.Id);
            }
            
            return users;
        }
        public UserDTO? GetUserById(int id)
        {
            var user = _userRepository.GetUserById(id);
            
            if (user != null)
            {
                // Populate statistics
                user.TotalObservations = _observationService.GetTotalObservationsByUser(user.Id);
                user.UniqueSpeciesObserved = _observationService.GetUniqueSpeciesCountByUser(user.Id);
            }
            
            return user;
        }
        public AuthResultDTO CreateUser(CreateUserDTO userDTO, IFormFile profilePicture)
        {
            PasswordHasher<object> passwordHasher = new PasswordHasher<object>();
            string passwordHash = passwordHasher.HashPassword(null, userDTO.Password);

            Task<string> imageUrl = _imageService.UploadAsync(profilePicture);

            string refreshToken = _authService.GenerateRefreshToken();
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            userDTO.Password = passwordHash;
            userDTO.ProfilePictureURL = imageUrl.Result;
            userDTO.RefreshToken = refreshToken;
            userDTO.RefreshTokenExpiry = refreshTokenExpiry;

            _userRepository.CreateUser(userDTO);

            UserDTO? user = _userRepository.GetUserByUsername(userDTO.Username);
            string accessToken = _authService.GenerateAccessToken(user);

            return new AuthResultDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public AuthResultDTO CreateUserSimple(CreateUserSimpleDTO userDTO)
        {
            PasswordHasher<object> passwordHasher = new PasswordHasher<object>();
            string passwordHash = passwordHasher.HashPassword(null, userDTO.Password);

            string refreshToken = _authService.GenerateRefreshToken();
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            // Convert to CreateUserDTO for repository
            var createUserDTO = new CreateUserDTO
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                Password = passwordHash,
                ProfilePictureURL = null, // No profile picture for simple creation
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry,
                CreatedAt = DateTime.UtcNow
            };

            _userRepository.CreateUser(createUserDTO);

            UserDTO? user = _userRepository.GetUserByUsername(userDTO.Username);
            string accessToken = _authService.GenerateAccessToken(user);

            return new AuthResultDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public bool DeleteUser(int id)
        {
            return _userRepository.DeleteUser(id);
        }

        public bool PatchUser(int id, PatchUserDTO dto, IFormFile profilePicture)
        {
            UserDTO existingUser = _userRepository.GetUserById(id);
            if (existingUser == null)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                PasswordHasher<object> hasher = new();
                dto.Password = hasher.HashPassword(null, dto.Password);
            }

            Task<string> imageUrl = _imageService.UploadAsync(profilePicture);
            dto.ProfilePictureURL = imageUrl.Result;

            return _userRepository.PatchUser(id, dto);
        }

        public UserDTO? GetUserByUsername(string username)
        {
            var user = _userRepository.GetUserByUsername(username.ToLower());
            
            if (user != null)
            {
                // Populate statistics
                user.TotalObservations = _observationService.GetTotalObservationsByUser(user.Id);
                user.UniqueSpeciesObserved = _observationService.GetUniqueSpeciesCountByUser(user.Id);
            }
            
            return user;
        }

        public UserDTO? GetUserByRefreshToken(string refreshToken)
        {
            var user = _userRepository.GetUserByRefreshToken(refreshToken);
            
            if (user != null)
            {
                // Populate statistics
                user.TotalObservations = _observationService.GetTotalObservationsByUser(user.Id);
                user.UniqueSpeciesObserved = _observationService.GetUniqueSpeciesCountByUser(user.Id);
            }
            
            return user;
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var passwordHasher = new PasswordHasher<object>();

            var result = passwordHasher.VerifyHashedPassword(null, passwordHash, password);

            return result == PasswordVerificationResult.Success;
        }

        public void UpdateRefreshToken(int userId, string refreshToken, DateTime expiry)
        {
            PatchUserDTO patchUserDTO = new PatchUserDTO
            {
                RefreshToken = refreshToken,
                RefreshTokenExpiry = expiry
            };

            _userRepository.PatchUser(userId, patchUserDTO);
        }

        public UserDTO GetUserByEmail(string email)
        {
            var user = _userRepository.GetUserByEmail(email.ToLower());
            
            if (user != null)
            {
                // Populate statistics
                user.TotalObservations = _observationService.GetTotalObservationsByUser(user.Id);
                user.UniqueSpeciesObserved = _observationService.GetUniqueSpeciesCountByUser(user.Id);
            }
            
            return user;
        }
    }
}
