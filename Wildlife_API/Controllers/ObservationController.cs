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
        public IActionResult GetAllObservations([FromQuery] int? limit = null)
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
            
            var observations = _observationService.GetAllObservations(actualLimit);
            return Ok(observations);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateObservation([FromForm] CreateObservationFormDTO dto, IFormFile? image = null)
        {
            int? userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized("User not authenticated");

            // Debug: Check if model binding worked
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Model validation failed", errors = errors });
            }

            // Debug: Log the received data
            Console.WriteLine($"Received DTO - SpeciesId: {dto.SpeciesId}, Body: {dto.Body}, DateObserved: {dto.DateObserved}, Lat: {dto.Latitude}, Lng: {dto.Longitude}");
            Console.WriteLine($"Image received: {image != null}, Image name: {image?.FileName}");

            // Validate species exists
            var speciesExists = await _context.Species.AnyAsync(s => s.Id == int.Parse(dto.SpeciesId));
            if (!speciesExists)
            {
                return BadRequest(new { message = $"Species with ID {dto.SpeciesId} does not exist" });
            }

            // Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value);
            if (!userExists)
            {
                return BadRequest(new { message = $"User with ID {userId.Value} does not exist" });
            }

            try
            {
                // Map form DTO to service DTO with proper type conversion
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
