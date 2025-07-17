var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Configure routing - Render platform constraint requires /health to be working route
app.MapRazorPages();
app.MapBlazorHub();

// Main application at root
app.MapFallbackToPage("/", "/_Host");

// Health endpoint (required by Render) that redirects to main app
app.MapGet("/health", () => Results.Redirect("/"));

app.Run();
