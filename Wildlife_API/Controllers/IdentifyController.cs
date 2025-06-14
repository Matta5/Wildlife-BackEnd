using Microsoft.AspNetCore.Mvc;
using Wildlife_BLL;
using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Wildlife_BackEnd.Controllers
{
    [ApiController]
    public class IdentifyController : ControllerBase
    {
        private readonly IdentifyService _identifyService;
        private readonly SpeciesService _speciesService;

        public IdentifyController(IdentifyService identifyService, SpeciesService speciesService)
        {
            _identifyService = identifyService;
            _speciesService = speciesService;
        }

        [HttpPost("identify")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<IdentifyResponseDTO>> Identify([FromForm] IdentifyRequestDTO request)
        {
            IdentifyResponseDTO result = await _identifyService.IdentifyAsync(request);

            if (result.Success)
            {
                await TryImportSpeciesAsync(result);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        [HttpPost("identify-base64")]
        public async Task<ActionResult<IdentifyResponseDTO>> IdentifyBase64([FromBody] IdentifyRequestDTO request)
        {
            IdentifyResponseDTO result = await _identifyService.IdentifyAsync(request);

            if (result.Success)
            {
                await TryImportSpeciesAsync(result);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        // Helper method to handle species import logic
        private async Task TryImportSpeciesAsync(IdentifyResponseDTO result)
        {
            if (string.IsNullOrEmpty(result.ScientificName))
                return;

            try
            {
                // Check if species exists locally
                var existingSpecies = await _speciesService.SearchSpeciesAsync(result.ScientificName, 1);
                
                if (!existingSpecies.Any(s => s.ScientificName.Equals(result.ScientificName, StringComparison.OrdinalIgnoreCase)))
                {
                    // Try to import using the taxon ID from the first result
                    var firstResult = result.AlternativeResults?.FirstOrDefault();
                    
                    if (firstResult?.TaxonId.HasValue == true)
                    {
                        // Direct import using taxon ID from identification result
                        var importedSpecies = await _speciesService.ImportSpeciesAsync(firstResult.TaxonId.Value);
                        if (importedSpecies != null)
                        {
                            result.ImportedSpeciesId = importedSpecies.Id;
                            result.ImportMessage = "Species automatically imported from iNaturalist";
                        }
                    }
                    else
                    {
                        // Fallback: search and import
                        var searchResults = await _speciesService.FindSpeciesAsync(result.ScientificName, 1);
                        
                        if (searchResults.Any())
                        {
                            var foundSpecies = searchResults.First();
                            // InaturalistTaxonId is not nullable, so we can use it directly
                            var importedSpecies = await _speciesService.ImportSpeciesAsync(foundSpecies.InaturalistTaxonId);
                            if (importedSpecies != null)
                            {
                                result.ImportedSpeciesId = importedSpecies.Id;
                                result.ImportMessage = "Species automatically imported from iNaturalist";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the identification
                Console.WriteLine($"Error importing species: {ex.Message}");
            }
        }
    }
}