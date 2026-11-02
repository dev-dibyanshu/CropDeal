using IdentityService.Models;
using IdentityService.Repositories;

namespace IdentityService.Data;

public class AdminSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AdminSeeder(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task SeedAdminAsync()
    {
        var adminConfig = _configuration.GetSection("AdminSeed");
        var adminEmail = adminConfig["Email"];

        if (string.IsNullOrEmpty(adminEmail))
        {
            return; // No admin configured
        }

        // Check if admin already exists (idempotent)
        var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            return; // Admin already exists
        }

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig["Password"]);

        // Create admin user
        var admin = new User
        {
            FullName = adminConfig["FullName"] ?? "System Admin",
            Email = adminEmail,
            MobileNumber = "0000000000", // Default mobile for admin
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.CreateAsync(admin);
    }
}
