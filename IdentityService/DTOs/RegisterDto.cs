using System.ComponentModel.DataAnnotations;
using IdentityService.Models;

namespace IdentityService.DTOs;

public class RegisterDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string MobileNumber { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; }
}
