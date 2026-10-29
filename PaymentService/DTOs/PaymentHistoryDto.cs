using PaymentService.Models;

namespace PaymentService.DTOs;

public class PaymentHistoryDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
