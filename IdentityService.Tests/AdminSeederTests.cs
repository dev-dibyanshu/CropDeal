using Moq;
using Microsoft.Extensions.Configuration;
using IdentityService.Data;
using IdentityService.Repositories;
using IdentityService.Models;

namespace IdentityService.Tests;

public class AdminSeederTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public AdminSeederTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    [Fact]
    public async Task SeedAdminAsync_WhenAdminDoesNotExist_ShouldCreateAdmin()
    {
        // Arrange
        var adminSection = new Mock<IConfigurationSection>();
        adminSection.Setup(x => x["FullName"]).Returns("System Admin");
        adminSection.Setup(x => x["Email"]).Returns("admin@cropdeal.com");
        adminSection.Setup(x => x["Password"]).Returns("Admin@123");

        _mockConfiguration.Setup(x => x.GetSection("AdminSeed")).Returns(adminSection.Object);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync("admin@cropdeal.com"))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync((User u) => u);

        var seeder = new AdminSeeder(_mockUserRepository.Object, _mockConfiguration.Object);

        // Act
        await seeder.SeedAdminAsync();

        // Assert
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        Assert.NotNull(capturedUser);
        Assert.Equal("System Admin", capturedUser.FullName);
        Assert.Equal("admin@cropdeal.com", capturedUser.Email);
        Assert.Equal(UserRole.Admin, capturedUser.Role);
        Assert.True(capturedUser.IsActive);
        
        // Verify password is hashed (not plain text)
        Assert.NotEqual("Admin@123", capturedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Admin@123", capturedUser.PasswordHash));
    }

    [Fact]
    public async Task SeedAdminAsync_WhenAdminAlreadyExists_ShouldNotCreateDuplicate()
    {
        // Arrange
        var adminSection = new Mock<IConfigurationSection>();
        adminSection.Setup(x => x["FullName"]).Returns("System Admin");
        adminSection.Setup(x => x["Email"]).Returns("admin@cropdeal.com");
        adminSection.Setup(x => x["Password"]).Returns("Admin@123");

        _mockConfiguration.Setup(x => x.GetSection("AdminSeed")).Returns(adminSection.Object);

        var existingAdmin = new User
        {
            Id = 1,
            FullName = "Existing Admin",
            Email = "admin@cropdeal.com",
            MobileNumber = "0000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync("admin@cropdeal.com"))
            .ReturnsAsync(existingAdmin);

        var seeder = new AdminSeeder(_mockUserRepository.Object, _mockConfiguration.Object);

        // Act
        await seeder.SeedAdminAsync();

        // Assert - Should NOT create a new user (idempotent)
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task SeedAdminAsync_WhenNoAdminConfigured_ShouldNotCreateAdmin()
    {
        // Arrange
        var adminSection = new Mock<IConfigurationSection>();
        adminSection.Setup(x => x["Email"]).Returns((string?)null); // No email configured

        _mockConfiguration.Setup(x => x.GetSection("AdminSeed")).Returns(adminSection.Object);

        var seeder = new AdminSeeder(_mockUserRepository.Object, _mockConfiguration.Object);

        // Act
        await seeder.SeedAdminAsync();

        // Assert - Should NOT attempt to query or create user
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task SeedAdminAsync_CreatedAdmin_ShouldHaveCorrectProperties()
    {
        // Arrange
        var adminSection = new Mock<IConfigurationSection>();
        adminSection.Setup(x => x["FullName"]).Returns("System Admin");
        adminSection.Setup(x => x["Email"]).Returns("admin@cropdeal.com");
        adminSection.Setup(x => x["Password"]).Returns("Admin@123");

        _mockConfiguration.Setup(x => x.GetSection("AdminSeed")).Returns(adminSection.Object);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync("admin@cropdeal.com"))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync((User u) => u);

        var seeder = new AdminSeeder(_mockUserRepository.Object, _mockConfiguration.Object);

        // Act
        await seeder.SeedAdminAsync();

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal("0000000000", capturedUser.MobileNumber); // Default mobile
        Assert.True(capturedUser.IsActive);
        Assert.Equal(UserRole.Admin, capturedUser.Role);
        Assert.True(capturedUser.CreatedAt <= DateTime.UtcNow);
    }
}
