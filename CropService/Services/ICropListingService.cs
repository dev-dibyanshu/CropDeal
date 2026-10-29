using CropService.DTOs;

namespace CropService.Services;

public interface ICropListingService
{
    Task<IEnumerable<CropListingResponseDto>> GetAllListingsAsync(string? category = null, string? cropName = null, string? location = null);
    Task<CropListingResponseDto?> GetListingByIdAsync(int id);
    Task<IEnumerable<CropListingResponseDto>> GetListingsByFarmerIdAsync(int farmerId);
    Task<(bool Success, string? Message, CropListingResponseDto? Listing)> CreateListingAsync(int farmerId, CreateCropListingDto createListingDto);
    Task<(bool Success, string? Message, CropListingResponseDto? Listing)> UpdateListingAsync(int id, int farmerId, UpdateCropListingDto updateListingDto);
}
