using Moq;
using OrderService.Services;
using OrderService.Repositories;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly IReviewService _reviewService;

    public ReviewServiceTests()
    {
        _mockReviewRepository = new Mock<IReviewRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _reviewService = new ReviewService(_mockReviewRepository.Object, _mockOrderRepository.Object);
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldCreateReview()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateReviewDto
        {
            OrderId = 1,
            FarmerId = 2,
            Rating = 5,
            Comment = "Excellent service"
        };

        var order = new Order
        {
            Id = createDto.OrderId,
            DealerId = dealerId,
            FarmerId = createDto.FarmerId,
            Status = OrderStatus.Completed
        };

        var createdReview = new Review
        {
            Id = 1,
            OrderId = createDto.OrderId,
            DealerId = dealerId,
            FarmerId = createDto.FarmerId,
            Rating = createDto.Rating,
            Comment = createDto.Comment
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(createDto.OrderId))
            .ReturnsAsync(order);

        _mockReviewRepository
            .Setup(x => x.GetByOrderIdAsync(createDto.OrderId))
            .ReturnsAsync((Review?)null);

        _mockReviewRepository
            .Setup(x => x.CreateAsync(It.IsAny<Review>()))
            .ReturnsAsync(createdReview);

        // Act
        var result = await _reviewService.CreateReviewAsync(dealerId, createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Review submitted successfully", result.Message);
        Assert.NotNull(result.Review);
        Assert.Equal(createDto.Rating, result.Review.Rating);
        _mockReviewRepository.Verify(x => x.CreateAsync(It.Is<Review>(r =>
            r.OrderId == createDto.OrderId &&
            r.Rating == createDto.Rating)), Times.Once);
    }

    [Fact]
    public async Task CreateReview_ForNonCompletedOrder_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateReviewDto
        {
            OrderId = 1,
            FarmerId = 2,
            Rating = 5,
            Comment = "Good"
        };

        var order = new Order
        {
            Id = createDto.OrderId,
            DealerId = dealerId,
            Status = OrderStatus.Pending
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(createDto.OrderId))
            .ReturnsAsync(order);

        // Act
        var result = await _reviewService.CreateReviewAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Reviews can only be submitted for completed orders", result.Message);
        Assert.Null(result.Review);
        _mockReviewRepository.Verify(x => x.CreateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task CreateReview_ForNonExistentOrder_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateReviewDto
        {
            OrderId = 999,
            FarmerId = 2,
            Rating = 5,
            Comment = "Good"
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(createDto.OrderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _reviewService.CreateReviewAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order not found", result.Message);
        Assert.Null(result.Review);
    }

    [Fact]
    public async Task CreateReview_ByUnauthorizedDealer_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var orderOwnerId = 2;
        var createDto = new CreateReviewDto
        {
            OrderId = 1,
            FarmerId = 3,
            Rating = 5,
            Comment = "Good"
        };

        var order = new Order
        {
            Id = createDto.OrderId,
            DealerId = orderOwnerId,
            Status = OrderStatus.Completed
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(createDto.OrderId))
            .ReturnsAsync(order);

        // Act
        var result = await _reviewService.CreateReviewAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Unauthorized to review this order", result.Message);
        _mockReviewRepository.Verify(x => x.CreateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task CreateReview_WhenReviewAlreadyExists_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateReviewDto
        {
            OrderId = 1,
            FarmerId = 2,
            Rating = 5,
            Comment = "Good"
        };

        var order = new Order
        {
            Id = createDto.OrderId,
            DealerId = dealerId,
            Status = OrderStatus.Completed
        };

        var existingReview = new Review
        {
            Id = 1,
            OrderId = createDto.OrderId,
            Rating = 4
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(createDto.OrderId))
            .ReturnsAsync(order);

        _mockReviewRepository
            .Setup(x => x.GetByOrderIdAsync(createDto.OrderId))
            .ReturnsAsync(existingReview);

        // Act
        var result = await _reviewService.CreateReviewAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Review already exists for this order", result.Message);
        _mockReviewRepository.Verify(x => x.CreateAsync(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task GetFarmerReviews_ShouldReturnAllReviews()
    {
        // Arrange
        var farmerId = 2;
        var reviews = new List<Review>
        {
            new Review { Id = 1, FarmerId = farmerId, Rating = 5 },
            new Review { Id = 2, FarmerId = farmerId, Rating = 4 }
        };

        _mockReviewRepository
            .Setup(x => x.GetByFarmerIdAsync(farmerId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetFarmerReviewsAsync(farmerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}
