namespace CropService.Models;

public class Crop
{
    public int Id { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<CropListing> CropListings { get; set; } = new List<CropListing>();
}
