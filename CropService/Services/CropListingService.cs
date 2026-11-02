using CropService.DTOs;
using CropService.Models;
using CropService.Repositories;

namespace CropService.Services;

public class CropListingService : ICropListingService
{
    private readonly ICropListingRepository _listingRepository;
    private readonly ICropRepository _cropRepository;

    public CropListingService(ICropListingRepository listingRepository, ICropRepository cropRepository)
    {
        _listingRepository = listingRepository;
        _cropRepository = cropRepository;
    }

    public async Task<IEnumerable<CropListingResponseDto>> GetAllListingsAsync(string? category = null, string? cropName = null, string? location = null)
    {
        var listings = await _listingRepository.GetAllAsync(category, cropName, location);
        return listings.Select(MapToResponseDto);
    }

    public async Task<CropListingResponseDto?> GetListingByIdAsync(int id)
    {
        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
        {
            return null;
        }

        return MapToResponseDto(listing);
    }

    public async Task<IEnumerable<CropListingResponseDto>> GetListingsByFarmerIdAsync(int farmerId)
    {
        var listings = await _listingRepository.GetByFarmerIdAsync(farmerId);
        return listings.Select(MapToResponseDto);
    }

    public async Task<(bool Success, string? Message, CropListingResponseDto? Listing)> CreateListingAsync(int farmerId, CreateCropListingDto createListingDto)
    {
        // Verify crop exists
        var crop = await _cropRepository.GetByIdAsync(createListingDto.CropId);
        if (crop == null)
        {
            return (false, "Crop not found", null);
        }

        var listing = new CropListing
        {
            FarmerId = farmerId,
            CropId = createListingDto.CropId,
            Quantity = createListingDto.Quantity,
            AvailableQuantity = createListingDto.Quantity,
            ExpectedPrice = createListingDto.ExpectedPrice,
            Location = createListingDto.Location,
            Status = CropListingStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        var createdListing = await _listingRepository.CreateAsync(listing);
        
        // Reload with crop data
        createdListing = await _listingRepository.GetByIdAsync(createdListing.Id);

        return (true, "Listing created successfully", MapToResponseDto(createdListing!));
    }

    public async Task<(bool Success, string? Message, CropListingResponseDto? Listing)> UpdateListingAsync(int id, int farmerId, UpdateCropListingDto updateListingDto)
    {
        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
        {
            return (false, "Listing not found", null);
        }

        // Check ownership
        if (listing.FarmerId != farmerId)
        {
            return (false, "Unauthorized to update this listing", null);
        }

        // Update fields if provided
        if (updateListingDto.Quantity.HasValue)
        {
            listing.Quantity = updateListingDto.Quantity.Value;
        }

        if (updateListingDto.AvailableQuantity.HasValue)
        {
            listing.AvailableQuantity = updateListingDto.AvailableQuantity.Value;
        }

        if (updateListingDto.ExpectedPrice.HasValue)
        {
            listing.ExpectedPrice = updateListingDto.ExpectedPrice.Value;
        }

        if (!string.IsNullOrWhiteSpace(updateListingDto.Location))
        {
            listing.Location = updateListingDto.Location;
        }

        if (updateListingDto.Status.HasValue)
        {
            listing.Status = updateListingDto.Status.Value;
        }

        var updatedListing = await _listingRepository.UpdateAsync(listing);

        return (true, "Listing updated successfully", MapToResponseDto(updatedListing));
    }

    private CropListingResponseDto MapToResponseDto(CropListing listing)
    {
        return new CropListingResponseDto
        {
            Id = listing.Id,
            FarmerId = listing.FarmerId,
            CropId = listing.CropId,
            CropName = listing.Crop.CropName,
            Category = listing.Crop.Category,
            Unit = listing.Crop.Unit,
            Quantity = listing.Quantity,
            AvailableQuantity = listing.AvailableQuantity,
            ExpectedPrice = listing.ExpectedPrice,
            Location = listing.Location,
            Status = listing.Status,
            CreatedAt = listing.CreatedAt
        };
    }

    public async Task<(bool Success, string? Message)> UpdateAvailableQuantityAsync(int id, decimal newAvailableQuantity)
    {
        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
        {
            return (false, "Listing not found");
        }

        if (newAvailableQuantity < 0)
        {
            return (false, "Available quantity cannot be negative");
        }

        listing.AvailableQuantity = newAvailableQuantity;
        await _listingRepository.UpdateAsync(listing);

        return (true, "Available quantity updated successfully");
    }
}
