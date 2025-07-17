var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Configure routing - Render requires health check at /app
app.MapRazorPages();
app.MapBlazorHub("/app/blazorhub");

// Map Blazor application to /app route (Render health check endpoint)
app.MapFallbackToPage("/app", "/_Host");

// Root redirect to /app for user convenience  
app.MapGet("/", () => Results.Redirect("/app"));

// Traditional health endpoint for monitoring
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
