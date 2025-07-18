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
        options.LoginPath = "/app";
        options.LogoutPath = "/app";
        options.AccessDeniedPath = "/app";
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

// Configure routing - Map everything to /app endpoint for Render compatibility
app.MapRazorPages();
app.MapBlazorHub("/app/blazorhub");

// Map Blazor application to /app route to avoid Render health check conflict
app.MapFallbackToPage("/app", "/_Host");

// Root redirect to /app for user convenience  
app.MapGet("/", () => Results.Redirect("/app"));

// Health check endpoint for Render
app.MapGet("/health", () => Results.Json(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
