namespace CropService.Models;

public class CropListing
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public int CropId { get; set; }
    public decimal Quantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ExpectedPrice { get; set; }
    public string Location { get; set; } = string.Empty;
    public CropListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Crop Crop { get; set; } = null!;
}
