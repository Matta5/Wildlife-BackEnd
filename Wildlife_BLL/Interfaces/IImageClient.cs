using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlife_BLL.Interfaces
{
    public interface IImageClient
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName);
    }
}

