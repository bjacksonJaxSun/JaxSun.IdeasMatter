using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jackson.Ideas.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse"),
            Items = { { "LoginProvider", "Google" } }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        try
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            
            if (!result.Succeeded)
            {
                _logger.LogWarning("Google authentication failed");
                return Redirect("/login");
            }

            var claims = result.Principal?.Claims;
            if (claims == null)
            {
                _logger.LogWarning("No claims received from Google authentication");
                return Redirect("/login");
            }

            // Extract user information from Google claims
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email claim not found in Google authentication response");
                return Redirect("/login");
            }

            // TODO: Integrate with your user service to create/authenticate user
            // For now, just redirect to dashboard
            _logger.LogInformation("Successfully authenticated user {Email} via Google", email);
            
            return Redirect("/dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during Google authentication");
            return Redirect("/login");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        await HttpContext.SignOutAsync("Google");
        return Redirect("/login");
    }
}