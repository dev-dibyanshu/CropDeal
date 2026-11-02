using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICropServiceClient _cropServiceClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ICropServiceClient cropServiceClient, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _cropServiceClient = cropServiceClient;
        _logger = logger;
    }

    public async Task<(bool Success, string? Message, OrderResponseDto? Order)> CreateOrderAsync(int dealerId, CreateOrderDto createOrderDto)
    {
        // Validate quantity is positive
        if (createOrderDto.Quantity <= 0)
        {
            return (false, "Quantity must be greater than 0", null);
        }

        // Get current crop listing from CropService
        var cropListing = await _cropServiceClient.GetCropListingAsync(createOrderDto.ListingId);
        if (cropListing == null)
        {
            return (false, "Crop listing not found", null);
        }

        // Validate requested quantity does not exceed available quantity
        if (createOrderDto.Quantity > cropListing.AvailableQuantity)
        {
            return (false, "Requested quantity exceeds available quantity", null);
        }

        // Create order
        var totalAmount = createOrderDto.Quantity * createOrderDto.Price;

        var order = new Order
        {
            ListingId = createOrderDto.ListingId,
            DealerId = dealerId,
            FarmerId = createOrderDto.FarmerId,
            Quantity = createOrderDto.Quantity,
            Price = createOrderDto.Price,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createdOrder = await _orderRepository.CreateAsync(order);

        // Update available quantity in CropService
        var newAvailableQuantity = cropListing.AvailableQuantity - createOrderDto.Quantity;
        var updated = await _cropServiceClient.UpdateListingQuantityAsync(createOrderDto.ListingId, newAvailableQuantity);
        
        if (!updated)
        {
            _logger.LogWarning("Failed to update available quantity for listing {ListingId}", createOrderDto.ListingId);
            // Note: Order was created but quantity update failed - this should be handled with proper transaction management
        }

        return (true, "Order created successfully", MapToResponseDto(createdOrder));
    }

    public async Task<IEnumerable<OrderResponseDto>> GetDealerOrdersAsync(int dealerId)
    {
        var orders = await _orderRepository.GetByDealerIdAsync(dealerId);
        return orders.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetFarmerOrdersAsync(int farmerId)
    {
        var orders = await _orderRepository.GetByFarmerIdAsync(farmerId);
        return orders.Select(MapToResponseDto);
    }

    public async Task<(bool Success, string? Message, OrderResponseDto? Order)> CancelOrderAsync(int orderId, int dealerId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return (false, "Order not found", null);
        }

        if (order.DealerId != dealerId)
        {
            return (false, "Unauthorized to cancel this order", null);
        }

        if (order.Status == OrderStatus.Completed)
        {
            return (false, "Cannot cancel a completed order", null);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return (false, "Order is already cancelled", null);
        }

        order.Status = OrderStatus.Cancelled;
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return (true, "Order cancelled successfully", MapToResponseDto(updatedOrder));
    }

    public async Task<(bool Success, string? Message, OrderResponseDto? Order)> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateStatusDto)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return (false, "Order not found", null);
        }

        order.Status = updateStatusDto.Status;
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return (true, "Order status updated successfully", MapToResponseDto(updatedOrder));
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return null;
        }

        return MapToResponseDto(order);
    }

    private OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            ListingId = order.ListingId,
            DealerId = order.DealerId,
            FarmerId = order.FarmerId,
            Quantity = order.Quantity,
            Price = order.Price,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }
}
