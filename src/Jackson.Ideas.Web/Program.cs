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
        options.LoginPath = "/health";
        options.LogoutPath = "/health";
        options.AccessDeniedPath = "/health";
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

// Configure routing - Map everything to /health endpoint (only working route on Render)
app.MapBlazorHub("/health/blazorhub");

// Map Blazor application to /health route 
app.MapRazorPages();
app.MapFallbackToPage("/health", "/_Host");

// Root redirect to /health for user convenience  
app.MapGet("/", () => Results.Redirect("/health"));

app.Run();
