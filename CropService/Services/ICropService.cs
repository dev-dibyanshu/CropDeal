using CropService.DTOs;

namespace CropService.Services;

public interface ICropService
{
    Task<IEnumerable<CropResponseDto>> GetAllCropsAsync();
    Task<CropResponseDto?> GetCropByIdAsync(int id);
    Task<CropResponseDto> CreateCropAsync(CreateCropDto createCropDto);
    Task<CropResponseDto?> UpdateCropAsync(int id, UpdateCropDto updateCropDto);
}
