using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wildlife_BLL.DTO;
using Wildlife_BLL;
using System.IdentityModel.Tokens.Jwt;

namespace Wildlife_BackEnd.Controllers
{
    [ApiController]
    [Route("observations")]
    public class ObservationController : ControllerBase
    {
        private readonly ObservationService _observationService;

        public ObservationController(ObservationService observationService)
        {
            _observationService = observationService;
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateObservation([FromBody] CreateEditObservationDTO dto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            _observationService.CreateObservation(userId.Value, dto);
            return Ok(new { message = "Observation created successfully" });
        }

        [HttpGet("{id}")]
        public IActionResult GetObservation(int id)
        {
            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound();

            return Ok(observation);
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return null;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }

    }
}
