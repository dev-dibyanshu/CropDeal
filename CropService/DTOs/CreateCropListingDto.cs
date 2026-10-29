using System.ComponentModel.DataAnnotations;

namespace CropService.DTOs;

public class CreateCropListingDto
{
    [Required]
    public int CropId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Expected price must be greater than or equal to 0")]
    public decimal ExpectedPrice { get; set; }
    
    [Required]
    public string Location { get; set; } = string.Empty;
}
