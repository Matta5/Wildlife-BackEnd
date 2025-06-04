using Microsoft.AspNetCore.Mvc;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;

namespace Wildlife_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentifyController : ControllerBase
    {
        private readonly IdentifyService _identifyService;

        public IdentifyController(IdentifyService identifyService)
        {
            _identifyService = identifyService;
        }

        [HttpPost("identify")]
        [Consumes("multipart/form-data", "application/json")]
        public async Task<ActionResult<IdentifyResponseDTO>> Identify([FromForm] IdentifyRequestDTO request)
        {
            IdentifyResponseDTO result = await _identifyService.IdentifyAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpPost("identify-base64")]
        public async Task<ActionResult<IdentifyResponseDTO>> IdentifyBase64([FromBody] IdentifyRequestDTO request)
        {
            IdentifyResponseDTO result = await _identifyService.IdentifyAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}