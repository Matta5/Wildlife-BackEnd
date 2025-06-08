using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.DTO
{
    public class IdentifyRequestDTO
    {
        public IFormFile? ImageFile { get; set; }
        public string EncodedImage { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
