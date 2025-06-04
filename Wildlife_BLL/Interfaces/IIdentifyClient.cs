using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlife_BLL.DTO;

namespace Wildlife_BLL.Interfaces
{
    public interface IIdentifyClient
    {
        Task<IdentifyResponseDTO> IdentifyAsync(byte[] imageBytes, double? latitude = null, double? longitude = null);
    }
}
