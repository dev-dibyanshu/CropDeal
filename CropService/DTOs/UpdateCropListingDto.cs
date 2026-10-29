using System.ComponentModel.DataAnnotations;
using CropService.Models;

namespace CropService.DTOs;

public class UpdateCropListingDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Available quantity must be greater than or equal to 0")]
    public decimal? AvailableQuantity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Expected price must be greater than or equal to 0")]
    public decimal? ExpectedPrice { get; set; }
    
    public string? Location { get; set; }
    
    public CropListingStatus? Status { get; set; }
}
