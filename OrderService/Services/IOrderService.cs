using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderService
{
    Task<(bool Success, string? Message, OrderResponseDto? Order)> CreateOrderAsync(int dealerId, CreateOrderDto createOrderDto);
    Task<IEnumerable<OrderResponseDto>> GetDealerOrdersAsync(int dealerId);
    Task<IEnumerable<OrderResponseDto>> GetFarmerOrdersAsync(int farmerId);
    Task<(bool Success, string? Message, OrderResponseDto? Order)> CancelOrderAsync(int orderId, int dealerId);
    Task<(bool Success, string? Message, OrderResponseDto? Order)> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateStatusDto);
    Task<OrderResponseDto?> GetOrderByIdAsync(int id);
}
