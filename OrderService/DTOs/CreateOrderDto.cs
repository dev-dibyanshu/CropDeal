using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class CreateOrderDto
{
    [Required]
    public int ListingId { get; set; }
    
    [Required]
    public int FarmerId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }
}
