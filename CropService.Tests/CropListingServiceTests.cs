using Moq;
using CropService.Services;
using CropService.Repositories;
using CropService.DTOs;
using CropService.Models;

namespace CropService.Tests;

public class CropListingServiceTests
{
    private readonly Mock<ICropListingRepository> _mockListingRepository;
    private readonly Mock<ICropRepository> _mockCropRepository;
    private readonly ICropListingService _listingService;

    public CropListingServiceTests()
    {
        _mockListingRepository = new Mock<ICropListingRepository>();
        _mockCropRepository = new Mock<ICropRepository>();
        _listingService = new CropListingService(_mockListingRepository.Object, _mockCropRepository.Object);
    }

    [Fact]
    public async Task GetAllListings_ShouldReturnAllListings()
    {
        // Arrange
        var listings = new List<CropListing>
        {
            new CropListing
            {
                Id = 1,
                FarmerId = 1,
                CropId = 1,
                Quantity = 100,
                AvailableQuantity = 100,
                ExpectedPrice = 50,
                Location = "Location1",
                Status = CropListingStatus.Available,
                Crop = new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg" }
            },
            new CropListing
            {
                Id = 2,
                FarmerId = 1,
                CropId = 2,
                Quantity = 200,
                AvailableQuantity = 200,
                ExpectedPrice = 60,
                Location = "Location2",
                Status = CropListingStatus.Available,
                Crop = new Crop { Id = 2, CropName = "Rice", Category = "Grain", Unit = "kg" }
            }
        };

        _mockListingRepository
            .Setup(x => x.GetAllAsync(null, null, null))
            .ReturnsAsync(listings);

        // Act
        var result = await _listingService.GetAllListingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockListingRepository.Verify(x => x.GetAllAsync(null, null, null), Times.Once);
    }

    [Fact]
    public async Task GetListingById_WithValidId_ShouldReturnListing()
    {
        // Arrange
        var listing = new CropListing
        {
            Id = 1,
            FarmerId = 1,
            CropId = 1,
            Quantity = 100,
            AvailableQuantity = 100,
            ExpectedPrice = 50,
            Location = "Location1",
            Status = CropListingStatus.Available,
            Crop = new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg" }
        };

        _mockListingRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(listing);

        // Act
        var result = await _listingService.GetListingByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(listing.Id, result.Id);
        Assert.Equal(listing.Crop.CropName, result.CropName);
    }

    [Fact]
    public async Task GetListingById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockListingRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((CropListing?)null);

        // Act
        var result = await _listingService.GetListingByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateListing_WithValidData_ShouldCreateListing()
    {
        // Arrange
        var farmerId = 1;
        var createDto = new CreateCropListingDto
        {
            CropId = 1,
            Quantity = 100,
            ExpectedPrice = 50,
            Location = "Test Location"
        };

        var crop = new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg" };

        var createdListing = new CropListing
        {
            Id = 1,
            FarmerId = farmerId,
            CropId = createDto.CropId,
            Quantity = createDto.Quantity,
            AvailableQuantity = createDto.Quantity,
            ExpectedPrice = createDto.ExpectedPrice,
            Location = createDto.Location,
            Status = CropListingStatus.Available,
            Crop = crop
        };

        _mockCropRepository
            .Setup(x => x.GetByIdAsync(createDto.CropId))
            .ReturnsAsync(crop);

        _mockListingRepository
            .Setup(x => x.CreateAsync(It.IsAny<CropListing>()))
            .ReturnsAsync(createdListing);

        _mockListingRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(createdListing);

        // Act
        var result = await _listingService.CreateListingAsync(farmerId, createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Listing created successfully", result.Message);
        Assert.NotNull(result.Listing);
        Assert.Equal(farmerId, result.Listing.FarmerId);
        _mockListingRepository.Verify(x => x.CreateAsync(It.Is<CropListing>(l =>
            l.FarmerId == farmerId &&
            l.CropId == createDto.CropId &&
            l.Quantity == createDto.Quantity)), Times.Once);
    }

    [Fact]
    public async Task CreateListing_WithInvalidCropId_ShouldFail()
    {
        // Arrange
        var farmerId = 1;
        var createDto = new CreateCropListingDto
        {
            CropId = 999,
            Quantity = 100,
            ExpectedPrice = 50,
            Location = "Test Location"
        };

        _mockCropRepository
            .Setup(x => x.GetByIdAsync(createDto.CropId))
            .ReturnsAsync((Crop?)null);

        // Act
        var result = await _listingService.CreateListingAsync(farmerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Crop not found", result.Message);
        Assert.Null(result.Listing);
        _mockListingRepository.Verify(x => x.CreateAsync(It.IsAny<CropListing>()), Times.Never);
    }

    [Fact]
    public async Task UpdateListing_WithValidData_ShouldUpdateListing()
    {
        // Arrange
        var listingId = 1;
        var farmerId = 1;
        var existingListing = new CropListing
        {
            Id = listingId,
            FarmerId = farmerId,
            CropId = 1,
            Quantity = 100,
            AvailableQuantity = 100,
            ExpectedPrice = 50,
            Location = "Old Location",
            Status = CropListingStatus.Available,
            Crop = new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg" }
        };

        var updateDto = new UpdateCropListingDto
        {
            Quantity = 150,
            ExpectedPrice = 60,
            Location = "New Location"
        };

        _mockListingRepository
            .Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(existingListing);

        _mockListingRepository
            .Setup(x => x.UpdateAsync(It.IsAny<CropListing>()))
            .ReturnsAsync((CropListing l) => l);

        // Act
        var result = await _listingService.UpdateListingAsync(listingId, farmerId, updateDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Listing updated successfully", result.Message);
        Assert.NotNull(result.Listing);
        _mockListingRepository.Verify(x => x.UpdateAsync(It.IsAny<CropListing>()), Times.Once);
    }

    [Fact]
    public async Task UpdateListing_WithInvalidId_ShouldFail()
    {
        // Arrange
        var listingId = 999;
        var farmerId = 1;
        var updateDto = new UpdateCropListingDto
        {
            Quantity = 150
        };

        _mockListingRepository
            .Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync((CropListing?)null);

        // Act
        var result = await _listingService.UpdateListingAsync(listingId, farmerId, updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Listing not found", result.Message);
        Assert.Null(result.Listing);
        _mockListingRepository.Verify(x => x.UpdateAsync(It.IsAny<CropListing>()), Times.Never);
    }

    [Fact]
    public async Task UpdateListing_WithDifferentFarmerId_ShouldFail()
    {
        // Arrange
        var listingId = 1;
        var ownerId = 1;
        var attemptingUserId = 2;
        var existingListing = new CropListing
        {
            Id = listingId,
            FarmerId = ownerId,
            CropId = 1,
            Quantity = 100,
            Crop = new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg" }
        };

        var updateDto = new UpdateCropListingDto
        {
            Quantity = 150
        };

        _mockListingRepository
            .Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(existingListing);

        // Act
        var result = await _listingService.UpdateListingAsync(listingId, attemptingUserId, updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Unauthorized to update this listing", result.Message);
        Assert.Null(result.Listing);
        _mockListingRepository.Verify(x => x.UpdateAsync(It.IsAny<CropListing>()), Times.Never);
    }

    [Fact]
    public async Task GetListingsByFarmerId_ShouldReturnFarmerListings()
    {
        // Arrange
        var farmerId = 1;
        var listings = new List<CropListing>
        {
            new CropListing
            {
                Id = 1,
                FarmerId = farmerId,
                CropId = 1,
                Quantity = 100,
                Crop = new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg" }
            }
        };

        _mockListingRepository
            .Setup(x => x.GetByFarmerIdAsync(farmerId))
            .ReturnsAsync(listings);

        // Act
        var result = await _listingService.GetListingsByFarmerIdAsync(farmerId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, l => Assert.Equal(farmerId, l.FarmerId));
    }
}
