using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<(bool Success, string? Message, OrderResponseDto? Order)> CreateOrderAsync(int dealerId, CreateOrderDto createOrderDto)
    {
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
