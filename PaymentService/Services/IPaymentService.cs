using PaymentService.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<(bool Success, string? Message, PaymentResponseDto? Payment)> ProcessPaymentAsync(int dealerId, CreatePaymentDto createPaymentDto);
    Task<IEnumerable<PaymentHistoryDto>> GetDealerPaymentHistoryAsync(int dealerId);
    Task<IEnumerable<PaymentHistoryDto>> GetFarmerPaymentHistoryAsync(int farmerId);
    Task<PaymentResponseDto?> GetPaymentByIdAsync(int id);
    Task<(bool Success, string? Message, PaymentResponseDto? Payment)> RefundPaymentAsync(int paymentId);
}
