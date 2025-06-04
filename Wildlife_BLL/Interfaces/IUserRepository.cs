using Wildlife_BLL.DTO;
using System.Threading.Tasks;

namespace Wildlife_BLL.Interfaces
{
    public interface IUserRepository
    {
        List<UserDTO> GetAllUsers();
        UserDTO GetUserById(int id);
        bool CreateUser(CreateUserDTO userDTO);
        bool DeleteUser(int id);
        bool PatchUser(int id, PatchUserDTO userDTO);
        UserDTO? GetUserByUsername(string username);
        UserDTO? GetUserByRefreshToken(string refreshToken);
        UserDTO? GetUserByEmail(string email);

    }
}
