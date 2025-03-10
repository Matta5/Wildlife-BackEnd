using Wildlife_BLL.DTO;
using System.Threading.Tasks;

namespace Wildlife_BLL.Interfaces
{
    public interface IUserRepository
    {
        public int GetByEmailOrUsernameAsync(string email, string username);
        public void CreateUserAsync(CreateEditUserDTO user); 
    }
}
