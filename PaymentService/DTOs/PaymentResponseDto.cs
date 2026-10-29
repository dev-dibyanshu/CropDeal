using PaymentService.Models;

namespace PaymentService.DTOs;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int DealerId { get; set; }
    public int FarmerId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
