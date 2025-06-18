using Wildlife_BLL.DTO;
using Wildlife_BLL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Wildlife_BLL
{
    public class ObservationService
    {
        private readonly IObservationRepository _observationRepository;
        private readonly ImageService _imageService;
        private readonly IObservationNotificationService _notificationService;

        public ObservationService(
            IObservationRepository observationRepository,
            ImageService imageService,
            IObservationNotificationService notificationService)
        {
            _observationRepository = observationRepository;
            _imageService = imageService;
            _notificationService = notificationService;
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

            var newObservationId = _observationRepository.CreateObservation(createEditObservationDTO);

            // Get the created observation to notify clients
            var createdObservation = _observationRepository.GetObservationById(newObservationId);
            if (createdObservation != null)
            {
                await _notificationService.NotifyObservationCreated(createdObservation);
            }
        }

        public async Task PatchObservation(int id, int userId, PatchObservationDTO patchDto, IFormFile? imageFile = null)
        {
            string? imageUrl = null;
            
            // Upload image if provided
            if (imageFile != null)
            {
                imageUrl = await _imageService.UploadAsync(imageFile);
                patchDto.ImageUrl = imageUrl;
            }

            var success = _observationRepository.PatchObservation(id, patchDto);

            if (success)
            {
                // Get the updated observation to notify clients
                var updatedObservation = _observationRepository.GetObservationById(id);
                if (updatedObservation != null)
                {
                    await _notificationService.NotifyObservationUpdated(updatedObservation);
                }
            }
        }

        public async Task DeleteObservation(int id)
        {
            var success = _observationRepository.DeleteObservation(id);
            
            if (success)
            {
                await _notificationService.NotifyObservationDeleted(id);
            }
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

        public List<ObservationDTO> GetAllObservations(int limit = 30, int? currentUserId = null, bool excludeCurrentUser = false)
        {
            return _observationRepository.GetAllObservations(limit, currentUserId, excludeCurrentUser);
        }
    }
}