﻿using System.Threading.Tasks;
using Wildlife_BLL.Interfaces;
using Wildlife_BLL.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Wildlife_BLL
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public UserService(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }
        public List<UserDTO> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }
        public UserDTO? GetUserById(int id)
        {
            return _userRepository.GetUserById(id);
        }
        public AuthResultDTO CreateUser(CreateEditUserDTO userDTO)
        {
            var existingUser = _userRepository.GetUserByUsername(userDTO.Username.ToLower());
            if (existingUser != null)
            {
                throw new Exception("Username already exists");
            }

            PasswordHasher<object> passwordHasher = new PasswordHasher<object>();
            string passwordHash = passwordHasher.HashPassword(null, userDTO.Password);

            string refreshToken = _authService.GenerateRefreshToken();
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            userDTO.Password = passwordHash;
            userDTO.RefreshToken = refreshToken;
            userDTO.RefreshTokenExpiry = refreshTokenExpiry;

            _userRepository.CreateUser(userDTO);

            UserDTO? user = _userRepository.GetUserByUsername(userDTO.Username); // Retrieve with original casing
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
        public bool UpdateUser(int id, CreateEditUserDTO userDTO)
        {
            PasswordHasher<object> passwordHasher = new PasswordHasher<object>();
            userDTO.Password = passwordHasher.HashPassword(null, userDTO.Password);

            return _userRepository.UpdateUser(id, userDTO);
        }

        public UserDTO? GetUserByUsername(string username)
        {
            return _userRepository.GetUserByUsername(username.ToLower()); // Normalize to lowercase
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
                RefreshToken = refreshToken,
                RefreshTokenExpiry = expiry
            };

            _userRepository.UpdateUser(user.Id, createEditUserDTO);
        }

    }
}
