using Jackson.Ideas.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Simple cookie authentication for mock environment
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.LogoutPath = "/";
        options.AccessDeniedPath = "/";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser());

// Application Services  
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IResearchSessionApiService, MockResearchSessionApiService>();

// Add navigation state service
builder.Services.AddSingleton<Jackson.Ideas.Web.Services.NavigationState>();

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Configure routing - Map everything to root endpoint
app.MapBlazorHub("/_blazor");

// Explicit health check endpoints for Render platform monitoring
app.MapGet("/health", () => Results.Json(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapGet("/healthz", () => Results.Json(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Map Blazor application to root route - MUST be last to avoid conflicts
app.MapRazorPages();
app.MapFallbackToPage("/", "/_Host");

app.Run();
