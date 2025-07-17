var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Configure routing - Render platform only allows /health route
app.MapRazorPages();
app.MapBlazorHub("/health/blazorhub");

// Map Blazor application to /health route (only working route on Render)
app.MapFallbackToPage("/health", "/_Host");

// Root redirect to /health for user convenience  
app.MapGet("/", () => Results.Redirect("/health"));

app.Run();
