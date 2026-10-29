using Moq;
using Microsoft.Extensions.Configuration;
using IdentityService.Services;
using IdentityService.Repositories;
using IdentityService.DTOs;
using IdentityService.Models;

namespace IdentityService.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup JWT configuration
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["Key"]).Returns("your-very-secure-secret-key-minimum-32-characters-long");
        jwtSection.Setup(x => x["Issuer"]).Returns("CropDealIdentity");
        jwtSection.Setup(x => x["Audience"]).Returns("CropDealUsers");
        jwtSection.Setup(x => x["ExpiryMinutes"]).Returns("60");

        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

        _authService = new AuthService(_mockUserRepository.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FullName = "John Doe",
            Email = "john@example.com",
            MobileNumber = "1234567890",
            Password = "password123",
            Role = UserRole.Farmer
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful", result.Message);
        Assert.Null(result.Response);
        _mockUserRepository.Verify(x => x.CreateAsync(It.Is<User>(u => 
            u.Email == registerDto.Email && 
            u.FullName == registerDto.FullName &&
            u.PasswordHash != registerDto.Password)), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WithExistingEmail_ShouldFail()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FullName = "John Doe",
            Email = "existing@example.com",
            MobileNumber = "1234567890",
            Password = "password123",
            Role = UserRole.Farmer
        };

        var existingUser = new User
        {
            Id = 1,
            Email = registerDto.Email,
            FullName = "Existing User",
            PasswordHash = "hashedpassword",
            Role = UserRole.Farmer,
            IsActive = true
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email already exists", result.Message);
        Assert.Null(result.Response);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUser_AsAdmin_ShouldFail()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FullName = "Admin User",
            Email = "admin@example.com",
            MobileNumber = "1234567890",
            Password = "password123",
            Role = UserRole.Admin
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cannot register as Admin", result.Message);
        Assert.Null(result.Response);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "password123"
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        var user = new User
        {
            Id = 1,
            Email = loginDto.Email,
            FullName = "John Doe",
            PasswordHash = passwordHash,
            Role = UserRole.Farmer,
            IsActive = true
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.NotNull(result.Response);
        Assert.NotNull(result.Response.Token);
        Assert.Equal(user.Id, result.Response.UserId);
        Assert.Equal(user.Email, result.Response.Email);
        Assert.Equal(user.FullName, result.Response.FullName);
        Assert.Equal(user.Role, result.Response.Role);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldFail()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "wrongpassword"
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User
        {
            Id = 1,
            Email = loginDto.Email,
            FullName = "John Doe",
            PasswordHash = passwordHash,
            Role = UserRole.Farmer,
            IsActive = true
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldFail()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task Login_WithInactiveAccount_ShouldFail()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "inactive@example.com",
            Password = "password123"
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        var user = new User
        {
            Id = 1,
            Email = loginDto.Email,
            FullName = "Inactive User",
            PasswordHash = passwordHash,
            Role = UserRole.Farmer,
            IsActive = false
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Account is inactive", result.Message);
        Assert.Null(result.Response);
    }
}
