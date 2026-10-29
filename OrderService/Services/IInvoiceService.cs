using OrderService.DTOs;

namespace OrderService.Services;

public interface IInvoiceService
{
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int id);
    Task<(bool Success, string? Message, InvoiceResponseDto? Invoice)> GenerateInvoiceAsync(int orderId);
}
