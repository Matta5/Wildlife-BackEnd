using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.DTO
{
    public class AuthenticatedUserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
