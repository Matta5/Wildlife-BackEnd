using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wildlife_BLL.DTO;
using Wildlife_BLL;
using System.IdentityModel.Tokens.Jwt;
using Wildlife_DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Wildlife_BackEnd.Controllers
{
    [ApiController]
    [Route("observations")]
    public class ObservationController : ControllerBase
    {
        private readonly ObservationService _observationService;
        private readonly AppDbContext _context;

        public ObservationController(ObservationService observationService, AppDbContext context)
        {
            _observationService = observationService;
            _context = context;
        }

        [HttpGet("GetAllFromUser/{id}")]
        public IActionResult GetObservationsByUser(int id)
        {
            var observations = _observationService.GetObservationsByUser(id);
            if (observations == null || !observations.Any())
                return NotFound("No observations found for this user");
            return Ok(observations);
        }

        [HttpGet]
        public IActionResult GetAllObservations([FromQuery] int? limit = null, [FromQuery] bool excludeCurrentUser = false)
        {
            // Ensure limit is divisible by 3, default to 30 if not specified
            int actualLimit = 30; // Default divisible by 3
            
            if (limit.HasValue)
            {
                // Round down to nearest multiple of 3
                actualLimit = (limit.Value / 3) * 3;
                
                // Ensure minimum of 3 and maximum of 300
                actualLimit = Math.Max(3, Math.Min(300, actualLimit));
            }
            
            int? currentUserId = null;
            if (excludeCurrentUser)
            {
                currentUserId = GetUserIdFromClaims();
            }
            
            var observations = _observationService.GetAllObservations(actualLimit, currentUserId, excludeCurrentUser);
            return Ok(observations);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateObservation([FromForm] CreateObservationFormDTO dto, IFormFile? image = null)
        {
            int? userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");


            var speciesExists = await _context.Species.AnyAsync(s => s.Id == int.Parse(dto.SpeciesId));
            if (!speciesExists)
            {
                return BadRequest(new { message = $"Species with ID {dto.SpeciesId} does not exist" });
            }

            try
            {
                var createObservationDTO = dto.ToCreateObservationDTO(userId.Value);

                await _observationService.CreateObservation(userId.Value, createObservationDTO, image);
                return Ok(new { message = "Observation created successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateObservation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to create observation", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("simple")]
        public IActionResult CreateObservationSimple([FromBody] CreateObservationSimpleDTO dto)
        {
            int? userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            try
            {
                _observationService.CreateObservationSimple(userId.Value, dto);
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
        public async Task<IActionResult> DeleteObservation(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound("Observation not found");

            if (observation.UserId != userId.Value)
            {
                Response.Headers.Add("X-Error-Message", "You do not have permission to delete this observation");
                return Forbid();
            }

            await _observationService.DeleteObservation(id);
            return Ok(new { message = "Observation deleted successfully" });
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PatchObservation(int id, [FromBody] PatchObservationDTO dto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            var observation = _observationService.GetObservationById(id);
            if (observation == null)
                return NotFound("Observation not found");

            if (observation.UserId != userId.Value)
            {
                Response.Headers.Add("X-Error-Message", "You do not have permission to update this observation");
                return Forbid();
            }

            await _observationService.PatchObservation(id, userId.Value, dto, null);
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
            {
                Response.Headers.Add("X-Error-Message", "You do not have permission to update this observation");
                return Forbid();
            }

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
