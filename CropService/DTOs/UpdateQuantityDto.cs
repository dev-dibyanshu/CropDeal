using System.ComponentModel.DataAnnotations;

namespace CropService.DTOs;

public class UpdateQuantityDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Available quantity must be greater than or equal to 0")]
    public decimal AvailableQuantity { get; set; }
}
