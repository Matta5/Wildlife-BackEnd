using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Wildlife_BLL
{
    public class ObservationService
    {
        private readonly IObservationRepository _observationRepository;
        private readonly ImageService _imageService;

        public ObservationService(IObservationRepository observationRepository, ImageService imageService)
        {
            _observationRepository = observationRepository;
            _imageService = imageService;
        }

        public async Task CreateObservation(int userId, CreateObservationDTO dto, IFormFile? imageFile = null)
        {
            string? imageUrl = null;
            
            // Upload image if provided
            if (imageFile != null)
            {
                imageUrl = await _imageService.UploadAsync(imageFile);
            }

            CreateObservationDTO createEditObservationDTO = new CreateObservationDTO
            {
                SpeciesId = dto.SpeciesId,
                UserId = userId,
                Body = dto.Body,
                DateObserved = dto.DateObserved,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImageUrl = imageUrl
            };
            _observationRepository.CreateObservation(createEditObservationDTO);
        }

        public void CreateObservationSimple(int userId, CreateObservationSimpleDTO dto)
        {
            CreateObservationDTO createObservationDTO = new CreateObservationDTO
            {
                SpeciesId = dto.SpeciesId,
                UserId = userId,
                Body = dto.Body,
                DateObserved = dto.DateObserved,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImageUrl = null // No image for simple creation
            };
            _observationRepository.CreateObservation(createObservationDTO);
        }

        public async Task<string?> UpdateObservationImage(int observationId, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No image file provided");

            // Upload the new image
            string imageUrl = await _imageService.UploadAsync(imageFile);
            
            // Update the observation with the new image URL
            var patchDto = new PatchObservationDTO
            {
                ImageUrl = imageUrl
            };
            
            _observationRepository.PatchObservation(observationId, patchDto);
            
            return imageUrl;
        }

        public bool DeleteObservation(int id)
        {
            return _observationRepository.DeleteObservation(id);
        }

        public ObservationDTO? GetObservationById(int id)
        {
            return _observationRepository.GetObservationById(id);
        }

        public List<ObservationDTO> GetObservationsByUser(int userId)
        {
            return _observationRepository.GetObservationsByUser(userId);
        }

        public bool PatchObservation(int value, PatchObservationDTO dto)
        {
            return _observationRepository.PatchObservation(value, dto);
        }

        public int GetTotalObservationsByUser(int userId)
        {
            return _observationRepository.GetTotalObservationsByUser(userId);
        }

        public int GetUniqueSpeciesCountByUser(int userId)
        {
            return _observationRepository.GetUniqueSpeciesCountByUser(userId);
        }

        public List<ObservationDTO> GetAllObservations(int limit = 30)
        {
            return _observationRepository.GetAllObservations(limit);
        }
    }

}