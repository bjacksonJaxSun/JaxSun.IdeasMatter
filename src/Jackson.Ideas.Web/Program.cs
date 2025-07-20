using Jackson.Ideas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
});

// Add HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Add HTTP Client
builder.Services.AddHttpClient();

// Add Authentication Services
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// Register NavigationState service
builder.Services.AddScoped<Jackson.Ideas.Web.Services.NavigationState>();

// Conditional service registration based on configuration
if (builder.Configuration.GetValue<bool>("UseMockAuthentication", true))
{
    builder.Services.AddScoped<IAuthenticationService, MockAuthenticationService>();
    builder.Services.AddScoped<IResearchSessionApiService, MockResearchSessionApiService>();
    builder.Services.AddScoped<IAdminService, MockAdminService>();
}
else
{
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IResearchSessionApiService, ResearchSessionApiService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
}

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Configure routing - Render requires health check at /app
app.MapRazorPages();
app.MapBlazorHub();

// Map all routes to Blazor application
app.MapFallbackToPage("/_Host");

// Traditional health endpoint for monitoring
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
