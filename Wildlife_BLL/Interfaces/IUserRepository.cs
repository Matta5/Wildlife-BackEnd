using Wildlife_BLL.DTO;
using System.Threading.Tasks;

namespace Wildlife_BLL.Interfaces
{
    public interface IUserRepository
    {
        List<UserDTO> GetAllUsers();
        UserDTO GetUserById(int id);
        bool CreateUser(CreateEditUserDTO userDTO);
        bool DeleteUser(int id);
        bool UpdateUser(int id, CreateEditUserDTO userDTO);
        UserDTO? GetUserByUsername(string username);
        UserDTO? GetUserByRefreshToken(string refreshToken);   

    }
}
