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

        [HttpGet("GetAllFromUser/{id}")]
        public IActionResult GetObservationsByUser(int id)
        {
            var observations = _observationService.GetObservationsByUser(id);
            if (observations == null || !observations.Any())
                return NotFound("No observations found for this user");
            return Ok(observations);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateObservation([FromForm] CreateObservationDTO dto, IFormFile? image = null)
        {
            int? userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            try
            {
                await _observationService.CreateObservation(userId.Value, dto, image);
                return Ok(new { message = "Observation created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create observation", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetObservation(int id)
        {
            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound();

            return Ok(observation);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteObservation(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound("Observation not found");

            if (observation.UserId != userId.Value)
                return Forbid("You do not have permission to delete this observation");

            var result = _observationService.DeleteObservation(id);

            if (!result)
                return BadRequest("Failed to delete observation");
            return Ok(new { message = "Observation deleted successfully" });
        }

        [HttpPatch("{id}")]
        [Authorize]
        public IActionResult PatchObservation(int id, [FromBody] PatchObservationDTO dto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound("Observation not found");

            if (observation.UserId != userId.Value)
                return Forbid("You do not have permission to update this observation");

            var result = _observationService.PatchObservation(id, dto);
            return Ok(new { message = "Observation updated successfully" });
        }

        [Authorize]
        [HttpPatch("{id}/image")]
        public async Task<IActionResult> UpdateObservationImage(int id, IFormFile image)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound("Observation not found");

            if (observation.UserId != userId.Value)
                return Forbid("You do not have permission to update this observation");

            try
            {
                var result = await _observationService.UpdateObservationImage(id, image);
                return Ok(new { message = "Observation image updated successfully", imageUrl = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update observation image", error = ex.Message });
            }
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
