namespace OrderService.DTOs;

public class CropListingDto
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public decimal Quantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
}
