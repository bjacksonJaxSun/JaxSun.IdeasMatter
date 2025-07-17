using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace Jackson.Ideas.Web.Services;

public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<JwtAuthenticationHandler> _logger;

    public JwtAuthenticationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<JwtAuthenticationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Get JWT token from cookie
            var token = httpContext.Request.Cookies["jwt"];
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Added JWT token to request for {RequestUri}", request.RequestUri);
            }
            else
            {
                _logger.LogDebug("No JWT token found for request to {RequestUri}", request.RequestUri);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}