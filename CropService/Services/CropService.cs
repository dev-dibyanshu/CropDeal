using CropService.DTOs;
using CropService.Models;
using CropService.Repositories;

namespace CropService.Services;

public class CropService : ICropService
{
    private readonly ICropRepository _cropRepository;

    public CropService(ICropRepository cropRepository)
    {
        _cropRepository = cropRepository;
    }

    public async Task<IEnumerable<CropResponseDto>> GetAllCropsAsync()
    {
        var crops = await _cropRepository.GetAllAsync();
        return crops.Select(c => new CropResponseDto
        {
            Id = c.Id,
            CropName = c.CropName,
            Category = c.Category,
            Unit = c.Unit,
            Description = c.Description
        });
    }

    public async Task<CropResponseDto?> GetCropByIdAsync(int id)
    {
        var crop = await _cropRepository.GetByIdAsync(id);
        if (crop == null)
        {
            return null;
        }

        return new CropResponseDto
        {
            Id = crop.Id,
            CropName = crop.CropName,
            Category = crop.Category,
            Unit = crop.Unit,
            Description = crop.Description
        };
    }

    public async Task<CropResponseDto> CreateCropAsync(CreateCropDto createCropDto)
    {
        var crop = new Crop
        {
            CropName = createCropDto.CropName,
            Category = createCropDto.Category,
            Unit = createCropDto.Unit,
            Description = createCropDto.Description
        };

        var createdCrop = await _cropRepository.CreateAsync(crop);

        return new CropResponseDto
        {
            Id = createdCrop.Id,
            CropName = createdCrop.CropName,
            Category = createdCrop.Category,
            Unit = createdCrop.Unit,
            Description = createdCrop.Description
        };
    }

    public async Task<CropResponseDto?> UpdateCropAsync(int id, UpdateCropDto updateCropDto)
    {
        var crop = await _cropRepository.GetByIdAsync(id);
        if (crop == null)
        {
            return null;
        }

        crop.CropName = updateCropDto.CropName;
        crop.Category = updateCropDto.Category;
        crop.Unit = updateCropDto.Unit;
        crop.Description = updateCropDto.Description;

        var updatedCrop = await _cropRepository.UpdateAsync(crop);

        return new CropResponseDto
        {
            Id = updatedCrop.Id,
            CropName = updatedCrop.CropName,
            Category = updatedCrop.Category,
            Unit = updatedCrop.Unit,
            Description = updatedCrop.Description
        };
    }
}
