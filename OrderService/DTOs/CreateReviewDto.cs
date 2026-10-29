using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs;

public class CreateReviewDto
{
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int FarmerId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [Required]
    public string Comment { get; set; } = string.Empty;
}
