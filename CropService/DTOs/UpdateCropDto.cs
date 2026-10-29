using System.ComponentModel.DataAnnotations;

namespace CropService.DTOs;

public class UpdateCropDto
{
    [Required]
    public string CropName { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public string Unit { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
}
