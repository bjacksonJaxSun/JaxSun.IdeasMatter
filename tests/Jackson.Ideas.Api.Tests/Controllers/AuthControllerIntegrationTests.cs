using Jackson.Ideas.Core.DTOs.Auth;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Jackson.Ideas.Api.Tests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the app DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<JacksonIdeasDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<JacksonIdeasDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // Build service provider and create database
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<JacksonIdeasDbContext>();
                dbContext.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResponse);
        Assert.True(authResponse.IsSuccess);
        Assert.NotNull(authResponse.AccessToken);
        Assert.NotNull(authResponse.RefreshToken);
        Assert.NotNull(authResponse.User);
        Assert.Equal(registerRequest.Email, authResponse.User.Email);
        Assert.Equal(registerRequest.Name, authResponse.User.Name);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Register first user
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Act - Try to register again with same email
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResponse);
        Assert.False(authResponse.IsSuccess);
        Assert.Contains("already exists", authResponse.Message);
    }

    [Theory]
    [InlineData("", "Test User", "TestPassword123", "TestPassword123")] // Empty email
    [InlineData("invalid-email", "Test User", "TestPassword123", "TestPassword123")] // Invalid email format
    [InlineData("test@example.com", "", "TestPassword123", "TestPassword123")] // Empty name
    [InlineData("test@example.com", "Test User", "", "TestPassword123")] // Empty password
    [InlineData("test@example.com", "Test User", "123", "123")] // Password too short
    [InlineData("test@example.com", "Test User", "TestPassword123", "DifferentPassword")] // Passwords don't match
    public async Task RegisterAsync_InvalidRequest_ShouldReturnBadRequest(
        string email, string name, string password, string confirmPassword)
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Name = name,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldCreateUserInDatabase()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "dbtest@example.com",
            Name = "Database Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify user was created in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JacksonIdeasDbContext>();
        
        var user = await dbContext.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email == registerRequest.Email);

        Assert.NotNull(user);
        Assert.Equal(registerRequest.Email, user.Email);
        Assert.Equal(registerRequest.Name, user.Name);
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(registerRequest.Password, user.PasswordHash); // Password should be hashed
        Assert.NotNull(user.RefreshToken);
        Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldReturnValidJwtToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "jwt@example.com",
            Name = "JWT Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResponse?.AccessToken);
        Assert.NotNull(authResponse?.RefreshToken);
        
        // JWT token should have 3 parts separated by dots
        var tokenParts = authResponse.AccessToken.Split('.');
        Assert.Equal(3, tokenParts.Length);
        
        // Each part should be base64 encoded
        foreach (var part in tokenParts)
        {
            Assert.True(part.Length > 0);
        }
    }
}