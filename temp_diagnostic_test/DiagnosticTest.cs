using Jackson.Ideas.Core.DTOs.Auth;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DiagnosticTest;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Auth Controller Diagnostic Test Runner ===");
        Console.WriteLine();

        int totalTests = 0;
        int passedTests = 0;

        // Test 1: Serialization/Deserialization test
        {
            totalTests++;
            Console.WriteLine("🧪 Test 1: JSON serialization/deserialization test");
            try
            {
                var request = new RegisterRequest
                {
                    Email = "test@example.com",
                    Name = "Test User",
                    Password = "TestPassword123",
                    ConfirmPassword = "TestPassword123"
                };

                var json = JsonSerializer.Serialize(request);
                var deserialized = JsonSerializer.Deserialize<RegisterRequest>(json);
                
                if (deserialized != null && 
                    deserialized.Email == request.Email &&
                    deserialized.Name == request.Name &&
                    deserialized.Password == request.Password &&
                    deserialized.ConfirmPassword == request.ConfirmPassword)
                {
                    Console.WriteLine("  ✅ PASSED - Serialization/Deserialization works correctly");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("  ❌ FAILED - Serialization/Deserialization data mismatch");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ FAILED - Serialization error: {ex.Message}");
            }
        }

        // Test 2: Password hashing edge cases
        {
            totalTests++;
            Console.WriteLine("🧪 Test 2: Password edge cases test");
            
            var testCases = new[]
            {
                "SimplePassword123",
                "ComplexP@ssw0rd!@#$%^&*()",
                new string('a', 99), // Near max length
                "Sp3c!@l#Ch@rs&*()+=[]{}|\\:;\"'<>,.?/~`"
            };

            bool allPassed = true;
            foreach (var password in testCases)
            {
                try
                {
                    var request = new RegisterRequest
                    {
                        Email = $"test{Guid.NewGuid()}@example.com",
                        Name = "Test User",
                        Password = password,
                        ConfirmPassword = password
                    };

                    var validationResults = ValidateModel(request);
                    
                    // For a valid password, should either pass validation or fail due to complexity rules
                    Console.WriteLine($"    Password test '{password.Substring(0, Math.Min(10, password.Length))}...' - Validation results: {validationResults.Count} errors");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ❌ Password test failed for: {password.Substring(0, Math.Min(10, password.Length))}... Error: {ex.Message}");
                    allPassed = false;
                }
            }

            if (allPassed)
            {
                Console.WriteLine("  ✅ PASSED - Password edge cases handled without exceptions");
                passedTests++;
            }
            else
            {
                Console.WriteLine("  ❌ FAILED - Some password edge cases threw exceptions");
            }
        }

        // Test 3: Model binding edge cases
        {
            totalTests++;
            Console.WriteLine("🧪 Test 3: Model binding edge cases test");
            
            bool allPassed = true;
            try
            {
                var edgeCaseRequests = new[]
                {
                    new RegisterRequest { Email = " ", Name = " ", Password = " ", ConfirmPassword = " " },
                    new RegisterRequest { Email = "\t\n\r", Name = "\t\n\r", Password = "\t\n\r", ConfirmPassword = "\t\n\r" },
                };

                foreach (var edgeRequest in edgeCaseRequests)
                {
                    var validationResults = ValidateModel(edgeRequest);
                    Console.WriteLine($"    Edge case validation results: {validationResults.Count} errors");
                }

                Console.WriteLine("  ✅ PASSED - Model binding edge cases handled without exceptions");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ FAILED - Model binding edge case failed: {ex.Message}");
            }
        }

        // Test 4: Common user errors
        {
            totalTests++;
            Console.WriteLine("🧪 Test 4: Common user errors test");
            
            var commonErrors = new[]
            {
                new { Email = "user@", Name = "Test User", Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Incomplete email" },
                new { Email = "@domain.com", Name = "Test User", Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Missing username" },
                new { Email = "test@example.com", Name = new string('a', 300), Password = "Test123", ConfirmPassword = "Test123", ExpectedIssue = "Name too long" },
            };

            bool allHandled = true;
            foreach (var errorCase in commonErrors)
            {
                try
                {
                    var request = new RegisterRequest
                    {
                        Email = errorCase.Email,
                        Name = errorCase.Name,
                        Password = errorCase.Password,
                        ConfirmPassword = errorCase.ConfirmPassword
                    };

                    var validationResults = ValidateModel(request);
                    Console.WriteLine($"    {errorCase.ExpectedIssue}: {validationResults.Count} validation errors");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    ❌ {errorCase.ExpectedIssue} caused exception: {ex.Message}");
                    allHandled = false;
                }
            }

            if (allHandled)
            {
                Console.WriteLine("  ✅ PASSED - Common user errors handled without exceptions");
                passedTests++;
            }
            else
            {
                Console.WriteLine("  ❌ FAILED - Some common user errors caused exceptions");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Diagnostic Test Summary ===");
        Console.WriteLine($"Total Tests: {totalTests}");
        Console.WriteLine($"Passed: {passedTests}");
        Console.WriteLine($"Failed: {totalTests - passedTests}");
        
        if (passedTests == totalTests)
        {
            Console.WriteLine("✅ All diagnostic tests PASSED!");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("❌ Some diagnostic tests FAILED!");
            Environment.Exit(1);
        }
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}