using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs;

public class CreatePaymentDto
{
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int FarmerId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    public string PaymentMethod { get; set; } = string.Empty;
}
