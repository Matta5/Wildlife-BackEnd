using Wildlife_BLL.Interfaces;
using Wildlife_DAL.Data;
using Wildlife_BLL.DTO;
using Wildlife_DAL.Entities;

namespace Wildlife_DAL;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<UserDTO> GetAllUsers()
    {
        try
        {
            return _context.Users.Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                ProfilePicture = u.ProfilePicture,
                CreatedAt = u.CreatedAt
            }).ToList();
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while getting all users", e);
        }
    }

    public UserDTO? GetUserById(int id)
    {
        try
        {
            UserEntity? user = _context.Users.Find(id);
            
            if (user == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while getting the user", e);
        }
    }

    public bool CreateUser(CreateEditUserDTO userDTO)
    {
        try
        {
            bool usernameExists = _context.Users.Any(u => u.Username == userDTO.Username);

            if (usernameExists)
            {
                throw new Exception($"Username with the name {userDTO.Username} already exists");
            }

            UserEntity user = new UserEntity
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                PasswordHash = userDTO.Password,
                ProfilePicture = userDTO.ProfilePicture,
                CreatedAt = userDTO.CreatedAt
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while creating user", e);
        }
    }

    public bool DeleteUser(int id)
    {
        try
        {
            UserEntity? user = _context.Users.Find(id);

            if (user == null)
            {
                throw new Exception($"User not found");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while deleting user", e);
        }        
    }

    public bool UpdateUser(int id, CreateEditUserDTO userDTO)
    {
        try
        {
            UserEntity? user = _context.Users.Find(id);

            if (user == null)
            {
                throw new Exception($"User not found");
            }

            user.Username = userDTO.Username;
            user.Email = userDTO.Email;
            user.PasswordHash = userDTO.Password;
            user.ProfilePicture = userDTO.ProfilePicture;
            user.CreatedAt = userDTO.CreatedAt;
            user.RefreshToken = userDTO.RefreshToken;
            user.RefreshTokenExpiry = userDTO.RefreshTokenExpiry;

            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while updating user", e);
        }
    }

    public UserDTO? GetUserByUsername(string username)
    {
        try
        {
            UserEntity? user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return null;
            }
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while getting the user by username", e);
        }
    }

    public UserDTO? GetUserByRefreshToken(string refreshToken)
    {
        try
        {
            UserEntity? user = _context.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
            if (user == null)
            {
                return null;
            }
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while getting the user by refresh token", e);
        }
    }
}
