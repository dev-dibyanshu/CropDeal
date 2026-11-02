using Moq;
using Microsoft.Extensions.Logging;
using OrderService.Services;
using OrderService.Repositories;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICropServiceClient> _mockCropServiceClient;
    private readonly Mock<ILogger<Services.OrderService>> _mockLogger;
    private readonly IOrderService _orderService;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCropServiceClient = new Mock<ICropServiceClient>();
        _mockLogger = new Mock<ILogger<Services.OrderService>>();
        _orderService = new Services.OrderService(
            _mockOrderRepository.Object, 
            _mockCropServiceClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrder_WithValidQuantity_ShouldCreateOrder()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateOrderDto
        {
            ListingId = 1,
            FarmerId = 2,
            Quantity = 10,
            Price = 50
        };

        var cropListing = new CropListingDto
        {
            Id = 1,
            FarmerId = 2,
            Quantity = 20,
            AvailableQuantity = 20
        };

        _mockCropServiceClient
            .Setup(x => x.GetCropListingAsync(createDto.ListingId))
            .ReturnsAsync(cropListing);

        _mockCropServiceClient
            .Setup(x => x.UpdateListingQuantityAsync(createDto.ListingId, 10))
            .ReturnsAsync(true);

        var createdOrder = new Order
        {
            Id = 1,
            ListingId = createDto.ListingId,
            DealerId = dealerId,
            FarmerId = createDto.FarmerId,
            Quantity = createDto.Quantity,
            Price = createDto.Price,
            TotalAmount = createDto.Quantity * createDto.Price,
            Status = OrderStatus.Pending
        };

        _mockOrderRepository
            .Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _orderService.CreateOrderAsync(dealerId, createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Order created successfully", result.Message);
        Assert.NotNull(result.Order);
        Assert.Equal(500, result.Order.TotalAmount);
        _mockOrderRepository.Verify(x => x.CreateAsync(It.Is<Order>(o =>
            o.DealerId == dealerId &&
            o.Quantity == createDto.Quantity &&
            o.Status == OrderStatus.Pending)), Times.Once);
        _mockCropServiceClient.Verify(x => x.UpdateListingQuantityAsync(createDto.ListingId, 10), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithExactAvailableQuantity_ShouldCreateOrder()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateOrderDto
        {
            ListingId = 1,
            FarmerId = 2,
            Quantity = 20,
            Price = 50
        };

        var cropListing = new CropListingDto
        {
            Id = 1,
            FarmerId = 2,
            Quantity = 20,
            AvailableQuantity = 20
        };

        _mockCropServiceClient
            .Setup(x => x.GetCropListingAsync(createDto.ListingId))
            .ReturnsAsync(cropListing);

        _mockCropServiceClient
            .Setup(x => x.UpdateListingQuantityAsync(createDto.ListingId, 0))
            .ReturnsAsync(true);

        var createdOrder = new Order
        {
            Id = 1,
            ListingId = createDto.ListingId,
            DealerId = dealerId,
            FarmerId = createDto.FarmerId,
            Quantity = createDto.Quantity,
            Price = createDto.Price,
            TotalAmount = createDto.Quantity * createDto.Price,
            Status = OrderStatus.Pending
        };

        _mockOrderRepository
            .Setup(x => x.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _orderService.CreateOrderAsync(dealerId, createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Order created successfully", result.Message);
        Assert.NotNull(result.Order);
        _mockOrderRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
        _mockCropServiceClient.Verify(x => x.UpdateListingQuantityAsync(createDto.ListingId, 0), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithQuantityExceedingAvailable_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateOrderDto
        {
            ListingId = 1,
            FarmerId = 2,
            Quantity = 50,
            Price = 50
        };

        var cropListing = new CropListingDto
        {
            Id = 1,
            FarmerId = 2,
            Quantity = 20,
            AvailableQuantity = 20
        };

        _mockCropServiceClient
            .Setup(x => x.GetCropListingAsync(createDto.ListingId))
            .ReturnsAsync(cropListing);

        // Act
        var result = await _orderService.CreateOrderAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Requested quantity exceeds available quantity", result.Message);
        Assert.Null(result.Order);
        _mockOrderRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);
        _mockCropServiceClient.Verify(x => x.UpdateListingQuantityAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WithZeroQuantity_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateOrderDto
        {
            ListingId = 1,
            FarmerId = 2,
            Quantity = 0,
            Price = 50
        };

        // Act
        var result = await _orderService.CreateOrderAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Quantity must be greater than 0", result.Message);
        Assert.Null(result.Order);
        _mockOrderRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);
        _mockCropServiceClient.Verify(x => x.GetCropListingAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WithNegativeQuantity_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateOrderDto
        {
            ListingId = 1,
            FarmerId = 2,
            Quantity = -5,
            Price = 50
        };

        // Act
        var result = await _orderService.CreateOrderAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Quantity must be greater than 0", result.Message);
        Assert.Null(result.Order);
        _mockOrderRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentListing_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreateOrderDto
        {
            ListingId = 999,
            FarmerId = 2,
            Quantity = 10,
            Price = 50
        };

        _mockCropServiceClient
            .Setup(x => x.GetCropListingAsync(createDto.ListingId))
            .ReturnsAsync((CropListingDto?)null);

        // Act
        var result = await _orderService.CreateOrderAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Crop listing not found", result.Message);
        Assert.Null(result.Order);
        _mockOrderRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            ListingId = 1,
            DealerId = 1,
            FarmerId = 2,
            Quantity = 100,
            Price = 50,
            TotalAmount = 5000,
            Status = OrderStatus.Pending
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.TotalAmount, result.TotalAmount);
    }

    [Fact]
    public async Task GetOrderById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelOrder_WithValidOrder_ShouldCancelOrder()
    {
        // Arrange
        var orderId = 1;
        var dealerId = 1;
        var existingOrder = new Order
        {
            Id = orderId,
            ListingId = 1,
            DealerId = dealerId,
            FarmerId = 2,
            Quantity = 100,
            Price = 50,
            TotalAmount = 5000,
            Status = OrderStatus.Pending
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, dealerId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Order cancelled successfully", result.Message);
        Assert.NotNull(result.Order);
        Assert.Equal(OrderStatus.Cancelled, result.Order.Status);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_WithInvalidId_ShouldFail()
    {
        // Arrange
        var orderId = 999;
        var dealerId = 1;

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, dealerId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order not found", result.Message);
        Assert.Null(result.Order);
    }

    [Fact]
    public async Task CancelOrder_WithCompletedStatus_ShouldFail()
    {
        // Arrange
        var orderId = 1;
        var dealerId = 1;
        var completedOrder = new Order
        {
            Id = orderId,
            DealerId = dealerId,
            Status = OrderStatus.Completed
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(completedOrder);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, dealerId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cannot cancel a completed order", result.Message);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_ByUnauthorizedDealer_ShouldFail()
    {
        // Arrange
        var orderId = 1;
        var ownerId = 1;
        var attemptingUserId = 2;
        var order = new Order
        {
            Id = orderId,
            DealerId = ownerId,
            Status = OrderStatus.Pending
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, attemptingUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Unauthorized to cancel this order", result.Message);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Confirmed
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, updateDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Order status updated successfully", result.Message);
        Assert.NotNull(result.Order);
        Assert.Equal(OrderStatus.Confirmed, result.Order.Status);
    }

    [Fact]
    public async Task GetDealerOrders_ShouldReturnDealerOrders()
    {
        // Arrange
        var dealerId = 1;
        var orders = new List<Order>
        {
            new Order { Id = 1, DealerId = dealerId, TotalAmount = 1000 },
            new Order { Id = 2, DealerId = dealerId, TotalAmount = 2000 }
        };

        _mockOrderRepository
            .Setup(x => x.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetDealerOrdersAsync(dealerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFarmerOrders_ShouldReturnFarmerOrders()
    {
        // Arrange
        var farmerId = 2;
        var orders = new List<Order>
        {
            new Order { Id = 1, FarmerId = farmerId, TotalAmount = 1000 }
        };

        _mockOrderRepository
            .Setup(x => x.GetByFarmerIdAsync(farmerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetFarmerOrdersAsync(farmerId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }
}
