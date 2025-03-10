using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_BLL.DTO;
using Wildlife_DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Wildlife_DAL
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByEmailOrUsernameAsync(string email, string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);
        }

        public async Task<UserEntity> CreateUserAsync(CreateEditUserDTO userDto)
        {
            var userEntity = new UserEntity
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = userDto.PasswordHash,
                ProfilePicture = userDto.ProfilePicture,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();
            return userEntity;
        }
    }
}
