using IdentityService.DTOs;

namespace IdentityService.Services;

public interface IAuthService
{
    Task<(bool Success, string? Message, LoginResponseDto? Response)> RegisterAsync(RegisterDto registerDto);
    Task<(bool Success, string? Message, LoginResponseDto? Response)> LoginAsync(LoginDto loginDto);
}
