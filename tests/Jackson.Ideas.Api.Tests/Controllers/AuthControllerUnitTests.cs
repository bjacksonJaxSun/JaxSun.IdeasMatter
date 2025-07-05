using Jackson.Ideas.Api.Controllers;
using Jackson.Ideas.Core.DTOs.Auth;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jackson.Ideas.Api.Tests.Controllers;

public class AuthControllerUnitTests
{
    private readonly Mock<IApplicationUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerUnitTests()
    {
        _mockUserRepository = new Mock<IApplicationUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        
        _controller = new AuthController(
            _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser user) => user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("mock-access-token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        var result = await _controller.RegisterAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        
        Assert.True(response.IsSuccess);
        Assert.Equal("Registration successful", response.Message);
        Assert.Equal("mock-access-token", response.AccessToken);
        Assert.Equal("mock-refresh-token", response.RefreshToken);
        Assert.NotNull(response.User);
        Assert.Equal(request.Email, response.User.Email);
        Assert.Equal(request.Name, response.User.Name);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        var existingUser = new ApplicationUser
        {
            Id = "existing-id",
            Email = request.Email,
            Name = "Existing User"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.RegisterAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(badRequestResult.Value);
        
        Assert.False(response.IsSuccess);
        Assert.Contains("already exists", response.Message);
        
        // Verify that AddAsync was never called
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_RepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.RegisterAsync(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        
        var response = Assert.IsType<AuthResponse>(statusCodeResult.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("error occurred", response.Message);
    }

    [Fact]
    public async Task RegisterAsync_JwtServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser user) => user);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Throws(new Exception("JWT generation failed"));

        // Act
        var result = await _controller.RegisterAsync(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserWithCorrectProperties()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        ApplicationUser? capturedUser = null;
        
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser user) => user)
            .Callback<ApplicationUser, CancellationToken>((user, ct) => capturedUser = user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("mock-access-token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        await _controller.RegisterAsync(request);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal(request.Email, capturedUser.Email);
        Assert.Equal(request.Name, capturedUser.Name);
        Assert.Equal(UserRole.User, capturedUser.Role);
        Assert.Equal("local", capturedUser.AuthProvider);
        Assert.False(capturedUser.IsVerified);
        Assert.NotNull(capturedUser.PasswordHash);
        Assert.NotEqual(request.Password, capturedUser.PasswordHash); // Should be hashed
        Assert.Equal("[]", capturedUser.Permissions);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_UpdatesUserWithRefreshToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        ApplicationUser? updatedUser = null;
        
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser user) => user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<ApplicationUser, CancellationToken>((user, ct) => updatedUser = user);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("mock-access-token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        await _controller.RegisterAsync(request);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal("mock-refresh-token", updatedUser.RefreshToken);
        Assert.True(updatedUser.RefreshTokenExpiryTime > DateTime.UtcNow);
        Assert.True(updatedUser.RefreshTokenExpiryTime <= DateTime.UtcNow.AddDays(7));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterAsync_InvalidEmail_ShouldBeHandledByModelValidation(string email)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Note: In a real scenario, model validation would catch this before reaching the controller
        // This test verifies the controller behavior if validation is bypassed

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        // The controller should handle null/empty emails gracefully
        await _controller.RegisterAsync(request);
        
        // Verify that the repository methods were still called with the invalid email
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_PasswordHashing_ShouldNotStoreRawPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "MySecretPassword123",
            ConfirmPassword = "MySecretPassword123"
        };

        ApplicationUser? capturedUser = null;
        
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser user) => user)
            .Callback<ApplicationUser, CancellationToken>((user, ct) => capturedUser = user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("mock-access-token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");

        // Act
        await _controller.RegisterAsync(request);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotNull(capturedUser.PasswordHash);
        Assert.NotEqual(request.Password, capturedUser.PasswordHash);
        Assert.True(capturedUser.PasswordHash.Length > request.Password.Length); // Hash should be longer
        
        // Verify password is not stored anywhere in plain text
        Assert.DoesNotContain(request.Password, capturedUser.PasswordHash);
    }
}