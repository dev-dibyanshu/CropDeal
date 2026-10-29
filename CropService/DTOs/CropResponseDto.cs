namespace CropService.DTOs;

public class CropResponseDto
{
    public int Id { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
