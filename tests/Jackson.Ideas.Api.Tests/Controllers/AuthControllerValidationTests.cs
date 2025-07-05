using Jackson.Ideas.Core.DTOs.Auth;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Jackson.Ideas.Api.Tests.Controllers;

public class AuthControllerValidationTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void RegisterRequest_ValidModel_ShouldPassValidation()
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RegisterRequest_EmptyOrNullEmail_ShouldFailValidation(string email)
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = email,
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Email"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid.email")]
    [InlineData("invalid@.com")]
    [InlineData("invalid@domain.")]
    public void RegisterRequest_InvalidEmailFormat_ShouldFailValidation(string email)
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = email,
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => 
            vr.MemberNames.Contains("Email") && 
            vr.ErrorMessage?.Contains("valid email") == true);
    }

    [Fact]
    public void RegisterRequest_EmailTooLong_ShouldFailValidation()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // Over 255 characters
        var model = new RegisterRequest
        {
            Email = longEmail,
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Email"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RegisterRequest_EmptyOrNullName_ShouldFailValidation(string name)
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = name,
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Name"));
    }

    [Fact]
    public void RegisterRequest_NameTooLong_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 256); // Over 255 characters
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = longName,
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RegisterRequest_EmptyOrNullPassword_ShouldFailValidation(string password)
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("a")]
    [InlineData("ab")]
    public void RegisterRequest_PasswordTooShort_ShouldFailValidation(string password)
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Password"));
    }

    [Fact]
    public void RegisterRequest_PasswordTooLong_ShouldFailValidation()
    {
        // Arrange
        var longPassword = new string('a', 101); // Over 100 characters
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = longPassword,
            ConfirmPassword = longPassword
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RegisterRequest_EmptyOrNullConfirmPassword_ShouldFailValidation(string confirmPassword)
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = confirmPassword
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains("ConfirmPassword"));
    }

    [Fact]
    public void RegisterRequest_PasswordsDoNotMatch_ShouldFailValidation()
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "DifferentPassword456"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, vr => 
            vr.MemberNames.Contains("ConfirmPassword") && 
            vr.ErrorMessage?.Contains("match") == true);
    }

    [Fact]
    public void RegisterRequest_SpecialCharactersInName_ShouldPassValidation()
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "José María O'Connor-Smith Jr.",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void RegisterRequest_ComplexValidEmail_ShouldPassValidation()
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "user.name+tag@example-domain.com",
            Name = "Test User",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void RegisterRequest_ComplexValidPassword_ShouldPassValidation()
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "MyComplex!Password123@#$",
            ConfirmPassword = "MyComplex!Password123@#$"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void RegisterRequest_UnicodeCharacters_ShouldPassValidation()
    {
        // Arrange
        var model = new RegisterRequest
        {
            Email = "тест@example.com",
            Name = "测试用户",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123"
        };

        // Act
        var validationResults = ValidateModel(model);

        // Assert
        // This test might fail if email validation doesn't support international domains
        // but the name should definitely pass
        var nameValidationErrors = validationResults.Where(vr => vr.MemberNames.Contains("Name"));
        Assert.Empty(nameValidationErrors);
    }
}