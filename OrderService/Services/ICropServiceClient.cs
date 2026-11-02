using OrderService.DTOs;

namespace OrderService.Services;

public interface ICropServiceClient
{
    Task<CropListingDto?> GetCropListingAsync(int listingId);
    Task<bool> UpdateListingQuantityAsync(int listingId, decimal newAvailableQuantity);
}
