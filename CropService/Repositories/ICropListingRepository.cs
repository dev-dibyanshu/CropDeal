using CropService.Models;

namespace CropService.Repositories;

public interface ICropListingRepository
{
    Task<IEnumerable<CropListing>> GetAllAsync(string? category = null, string? cropName = null, string? location = null);
    Task<CropListing?> GetByIdAsync(int id);
    Task<IEnumerable<CropListing>> GetByFarmerIdAsync(int farmerId);
    Task<CropListing> CreateAsync(CropListing listing);
    Task<CropListing> UpdateAsync(CropListing listing);
}
