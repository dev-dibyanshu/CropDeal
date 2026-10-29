using CropService.Models;

namespace CropService.DTOs;

public class CropListingResponseDto
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public int CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ExpectedPrice { get; set; }
    public string Location { get; set; } = string.Empty;
    public CropListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
