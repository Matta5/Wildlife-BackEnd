using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces
{
    public interface IAuthService
    {
        string GenerateAccessToken(UserDTO user);
        string GenerateRefreshToken();
    }
}
