using Jackson.Ideas.Core.Entities;
using System.Security.Claims;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user);
    
    string GenerateRefreshToken();
    
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    
    bool ValidateToken(string token);
    
    string GetUserIdFromToken(string token);
    
    DateTime? GetTokenExpiration(string token);
    
    bool IsTokenExpired(string token);
}