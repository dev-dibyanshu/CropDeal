using OrderService.DTOs;

namespace OrderService.Services;

public interface IReceiptService
{
    Task<ReceiptResponseDto?> GetReceiptByIdAsync(int id);
    Task<(bool Success, string? Message, ReceiptResponseDto? Receipt)> GenerateReceiptAsync(int orderId);
}
