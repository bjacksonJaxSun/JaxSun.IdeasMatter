using Jackson.Ideas.Api.Controllers;
using Jackson.Ideas.Core.DTOs.Auth;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Jackson.Ideas.Api.Tests.Controllers;

/// <summary>
/// Diagnostic tests to identify and replicate common registration errors
/// These tests are designed to catch real-world issues that users might encounter
/// </summary>
public class AuthControllerErrorDiagnosticTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IApplicationUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IPasswordHasher<ApplicationUser>> _mockPasswordHasher;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly AuthController _controller;

    public AuthControllerErrorDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
        _mockUserRepository = new Mock<IApplicationUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockPasswordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        
        var mockStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(mockStore.Object, null, null, null, null, null, null, null, null);
        
        _controller = new AuthController(
            _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockLogger.Object,
            _mockPasswordHasher.Object,
            _mockUserManager.Object);
    }

    [Fact]
    public async Task DiagnosticTest_IdentifyNullReferenceErrors()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing for potential null reference exceptions");

        // Test Case 1: Repository returns null but service expects non-null
        var request = CreateValidRequest();
        
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // This could cause null reference if not handled properly
        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null); // Returning null instead of user

        var result = await _controller.RegisterAsync(request);
        
        _output.WriteLine($"Result type: {result.Result?.GetType().Name}");
        
        // Should handle gracefully without null reference exception
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task DiagnosticTest_IdentifySerializationErrors()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing JSON serialization/deserialization issues");

        var request = CreateValidRequest();
        
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockUser());

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockJwtService
            .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("valid.jwt.token");

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("valid-refresh-token");

        var result = await _controller.RegisterAsync(request);
        
        // Test serialization of the response
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        
        try
        {
            var json = JsonSerializer.Serialize(response);
            _output.WriteLine($"Serialized successfully: {json.Length} characters");
            
            var deserialized = JsonSerializer.Deserialize<AuthResponse>(json);
            Assert.NotNull(deserialized);
            _output.WriteLine("✅ Serialization/Deserialization works correctly");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Serialization error: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task DiagnosticTest_IdentifyDatabaseConstraintViolations()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing database constraint violations");

        var request = CreateValidRequest();
        
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Simulate unique constraint violation (duplicate email)
        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Violation of UNIQUE KEY constraint 'IX_Users_Email'"));

        var result = await _controller.RegisterAsync(request);
        
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        
        var response = Assert.IsType<AuthResponse>(statusResult.Value);
        Assert.False(response.IsSuccess);
        _output.WriteLine($"✅ Handled database constraint violation: {response.Message}");
    }

    [Fact]
    public async Task DiagnosticTest_IdentifyPasswordHashingIssues()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing password hashing edge cases");

        // Test various password scenarios that might cause hashing issues
        var testCases = new[]
        {
            "SimplePassword123",
            "ComplexP@ssw0rd!@#$%^&*()",
            "UnicodeПароль123",
            "EmptyStringTest" + char.MinValue,
            new string('a', 99), // Near max length
            "Sp3c!@l#Ch@rs&*()+=[]{}|\\:;\"'<>,.?/~`"
        };

        foreach (var password in testCases)
        {
            var request = new RegisterRequest
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                Name = "Test User",
                Password = password,
                ConfirmPassword = password
            };

            ApplicationUser? capturedUser = null;
            
            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
                .Returns("mock-token");

            _mockJwtService
                .Setup(x => x.GenerateRefreshToken())
                .Returns("mock-refresh");

            try
            {
                var result = await _controller.RegisterAsync(request);
                
                Assert.NotNull(capturedUser);
                Assert.NotNull(capturedUser.PasswordHash);
                Assert.NotEqual(password, capturedUser.PasswordHash);
                
                _output.WriteLine($"✅ Password hashing successful for: {password.Substring(0, Math.Min(10, password.Length))}...");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ Password hashing failed for: {password.Substring(0, Math.Min(10, password.Length))}... Error: {ex.Message}");
                throw;
            }
        }
    }

    [Fact]
    public async Task DiagnosticTest_IdentifyJwtGenerationIssues()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing JWT generation edge cases");

        var request = CreateValidRequest();
        
        // Test various user scenarios that might cause JWT issues
        var problematicUsers = new[]
        {
            new ApplicationUser { Id = "", Email = request.Email, Name = request.Name }, // Empty ID
            new ApplicationUser { Id = "valid-id", Email = "", Name = request.Name }, // Empty email
            new ApplicationUser { Id = "valid-id", Email = request.Email, Name = "" }, // Empty name
            new ApplicationUser { Id = new string('a', 450), Email = request.Email, Name = request.Name }, // Very long ID
            new ApplicationUser { Id = "valid-id", Email = request.Email + new string('a', 200), Name = request.Name }, // Very long email
        };

        foreach (var problematicUser in problematicUsers)
        {
            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApplicationUser?)null);

            _mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(problematicUser);

            _mockUserRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            try
            {
                _mockJwtService
                    .Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
                    .Returns("mock-token");

                _mockJwtService
                    .Setup(x => x.GenerateRefreshToken())
                    .Returns("mock-refresh");

                var result = await _controller.RegisterAsync(request);
                _output.WriteLine($"✅ JWT generation handled user with ID length: {problematicUser.Id?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ JWT generation failed for problematic user: {ex.Message}");
                
                // This might be expected for some edge cases
                var result = await _controller.RegisterAsync(request);
                var statusResult = Assert.IsType<ObjectResult>(result.Result);
                Assert.Equal(500, statusResult.StatusCode);
            }
        }
    }

    [Fact]
    public async Task DiagnosticTest_IdentifyModelBindingIssues()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing model binding edge cases");

        // Test cases that might cause model binding issues
        var edgeCaseRequests = new[]
        {
            new RegisterRequest { Email = null!, Name = null!, Password = null!, ConfirmPassword = null! },
            new RegisterRequest { Email = " ", Name = " ", Password = " ", ConfirmPassword = " " },
            new RegisterRequest { Email = "\t\n\r", Name = "\t\n\r", Password = "\t\n\r", ConfirmPassword = "\t\n\r" },
        };

        foreach (var edgeRequest in edgeCaseRequests)
        {
            try
            {
                _mockUserRepository
                    .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ApplicationUser?)null);

                var result = await _controller.RegisterAsync(edgeRequest);
                
                _output.WriteLine($"✅ Model binding handled edge case without exception");
                
                // Should return some form of error response
                Assert.NotNull(result.Result);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ Model binding failed: {ex.Message}");
                throw;
            }
        }
    }

    [Fact]
    public async Task DiagnosticTest_CheckCommonUserErrors()
    {
        _output.WriteLine("🔍 DIAGNOSTIC: Testing common user input errors");

        var commonErrors = new[]
        {
            // Email errors
            new { Email = "user@", Name = "Test User", Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Incomplete email" },
            new { Email = "@domain.com", Name = "Test User", Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Missing username" },
            new { Email = "user@domain", Name = "Test User", Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Missing TLD" },
            
            // Password errors
            new { Email = "test@example.com", Name = "Test User", Password = "123", ConfirmPassword = "123", ExpectedIssue = "Password too short" },
            new { Email = "test@example.com", Name = "Test User", Password = "password", ConfirmPassword = "PASSWORD", ExpectedIssue = "Case mismatch" },
            new { Email = "test@example.com", Name = "Test User", Password = "Test123", ConfirmPassword = "Test124", ExpectedIssue = "Passwords don't match" },
            
            // Name errors
            new { Email = "test@example.com", Name = new string('a', 300), Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Name too long" },
        };

        foreach (var errorCase in commonErrors)
        {
            var request = new RegisterRequest
            {
                Email = errorCase.Email,
                Name = errorCase.Name,
                Password = errorCase.Password,
                ConfirmPassword = errorCase.ConfirmPassword
            };

            _mockUserRepository
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.RegisterAsync(request);
            
            _output.WriteLine($"Tested: {errorCase.ExpectedIssue} - Result: {result.Result?.GetType().Name}");
        }
    }

    private static RegisterRequest CreateValidRequest()
    {
        return new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };
    }

    private static ApplicationUser CreateMockUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed-password",
            CreatedAt = DateTime.UtcNow
        };
    }
}