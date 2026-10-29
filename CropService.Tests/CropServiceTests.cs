using Moq;
using CropService.Services;
using CropService.Repositories;
using CropService.DTOs;
using CropService.Models;

namespace CropService.Tests;

public class CropServiceTests
{
    private readonly Mock<ICropRepository> _mockCropRepository;
    private readonly ICropService _cropService;

    public CropServiceTests()
    {
        _mockCropRepository = new Mock<ICropRepository>();
        _cropService = new Services.CropService(_mockCropRepository.Object);
    }

    [Fact]
    public async Task GetAllCrops_ShouldReturnAllCrops()
    {
        // Arrange
        var crops = new List<Crop>
        {
            new Crop { Id = 1, CropName = "Wheat", Category = "Grain", Unit = "kg", Description = "Winter wheat" },
            new Crop { Id = 2, CropName = "Rice", Category = "Grain", Unit = "kg", Description = "Basmati rice" }
        };

        _mockCropRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(crops);

        // Act
        var result = await _cropService.GetAllCropsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockCropRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCropById_WithValidId_ShouldReturnCrop()
    {
        // Arrange
        var crop = new Crop
        {
            Id = 1,
            CropName = "Wheat",
            Category = "Grain",
            Unit = "kg",
            Description = "Winter wheat"
        };

        _mockCropRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(crop);

        // Act
        var result = await _cropService.GetCropByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(crop.Id, result.Id);
        Assert.Equal(crop.CropName, result.CropName);
        Assert.Equal(crop.Category, result.Category);
    }

    [Fact]
    public async Task GetCropById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockCropRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Crop?)null);

        // Act
        var result = await _cropService.GetCropByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCrop_WithValidData_ShouldCreateCrop()
    {
        // Arrange
        var createDto = new CreateCropDto
        {
            CropName = "Tomato",
            Category = "Vegetable",
            Unit = "kg",
            Description = "Fresh tomatoes"
        };

        var createdCrop = new Crop
        {
            Id = 1,
            CropName = createDto.CropName,
            Category = createDto.Category,
            Unit = createDto.Unit,
            Description = createDto.Description
        };

        _mockCropRepository
            .Setup(x => x.CreateAsync(It.IsAny<Crop>()))
            .ReturnsAsync(createdCrop);

        // Act
        var result = await _cropService.CreateCropAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.CropName, result.CropName);
        Assert.Equal(createDto.Category, result.Category);
        _mockCropRepository.Verify(x => x.CreateAsync(It.Is<Crop>(c =>
            c.CropName == createDto.CropName &&
            c.Category == createDto.Category)), Times.Once);
    }

    [Fact]
    public async Task UpdateCrop_WithValidId_ShouldUpdateCrop()
    {
        // Arrange
        var existingCrop = new Crop
        {
            Id = 1,
            CropName = "Wheat",
            Category = "Grain",
            Unit = "kg",
            Description = "Winter wheat"
        };

        var updateDto = new UpdateCropDto
        {
            CropName = "Wheat Premium",
            Category = "Grain",
            Unit = "kg",
            Description = "Premium winter wheat"
        };

        _mockCropRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingCrop);

        _mockCropRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Crop>()))
            .ReturnsAsync((Crop c) => c);

        // Act
        var result = await _cropService.UpdateCropAsync(1, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.CropName, result.CropName);
        Assert.Equal(updateDto.Description, result.Description);
        _mockCropRepository.Verify(x => x.UpdateAsync(It.IsAny<Crop>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCrop_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var updateDto = new UpdateCropDto
        {
            CropName = "Wheat Premium",
            Category = "Grain",
            Unit = "kg",
            Description = "Premium winter wheat"
        };

        _mockCropRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Crop?)null);

        // Act
        var result = await _cropService.UpdateCropAsync(999, updateDto);

        // Assert
        Assert.Null(result);
        _mockCropRepository.Verify(x => x.UpdateAsync(It.IsAny<Crop>()), Times.Never);
    }
}
