// Wildlife_API/Controllers/SpeciesController.cs
using Microsoft.AspNetCore.Mvc;
using Wildlife_BLL.DTO;
using Wildlife_BLL;
using Wildlife_BLL.Interfaces;
using System.Net.Http;

namespace Wildlife_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly SpeciesService _speciesService;

    public SpeciesController(SpeciesService speciesService)
    {
        _speciesService = speciesService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpeciesDTO>> GetSpecies(int id)
    {
        var species = await _speciesService.GetSpeciesByIdAsync(id);

        if (species == null)
            return NotFound($"Species with ID {id} not found");

        return Ok(species);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<SpeciesDTO>>> SearchSpecies(
        [FromQuery] string q,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query cannot be empty");

        if (limit > 100)
            limit = 100;

        var results = await _speciesService.SearchSpeciesAsync(q, limit);
        return Ok(results);
    }

    [HttpGet("find")]
    public async Task<ActionResult<List<SpeciesDTO>>> FindSpecies([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query cannot be empty");

        if (limit > 50)
            limit = 50;

        var species = await _speciesService.FindSpeciesAsync(q, limit);

        if (!species.Any())
            return NotFound($"No species found for '{q}' in local database or iNaturalist");

        return Ok(species);
    }

    [HttpPost("import/{taxonId:long}")]
    public async Task<ActionResult<SpeciesDTO>> ImportSpecies(long taxonId)
    {
        var species = await _speciesService.ImportSpeciesByTaxonIdAsync(taxonId);

        if (species == null)
            return NotFound($"Species with taxon ID {taxonId} not found on iNaturalist");

        return Ok(species);
    }

    [HttpGet("debug/inaturalist/{taxonId:long}")]
    public async Task<ActionResult> DebugInaturalist(long taxonId)
    {
        try
        {
            var url = $"https://api.inaturalist.org/v1/taxa/{taxonId}";
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "WildlifeApp/1.0");
            
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            return Ok(new 
            {
                StatusCode = (int)response.StatusCode,
                StatusMessage = response.StatusCode.ToString(),
                Content = content,
                ContentLength = content.Length
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("popular")]
    public async Task<ActionResult<List<SpeciesDTO>>> GetPopularSpecies(
        [FromQuery] int limit = 50)
    {
        if (limit > 200)
            limit = 200;

        var species = await _speciesService.GetPopularDutchSpeciesAsync(limit);
        return Ok(species);
    }

    [HttpGet("class/{className}")]
    public async Task<ActionResult<List<SpeciesDTO>>> GetSpeciesByClass(
        string className,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(className))
            return BadRequest("Class name cannot be empty");

        if (limit > 100)
            limit = 100;

        var species = await _speciesService.GetSpeciesByClassAsync(className, limit);
        return Ok(species);
    }

    [HttpGet("order/{orderName}")]
    public async Task<ActionResult<List<SpeciesDTO>>> GetSpeciesByOrder(
        string orderName,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(orderName))
            return BadRequest("Order name cannot be empty");

        if (limit > 100)
            limit = 100;

        var species = await _speciesService.GetSpeciesByOrderAsync(orderName, limit);
        return Ok(species);
    }

    [HttpGet("family/{familyName}")]
    public async Task<ActionResult<List<SpeciesDTO>>> GetSpeciesByFamily(
        string familyName,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(familyName))
            return BadRequest("Family name cannot be empty");

        if (limit > 100)
            limit = 100;

        var species = await _speciesService.GetSpeciesByFamilyAsync(familyName, limit);
        return Ok(species);
    }
}