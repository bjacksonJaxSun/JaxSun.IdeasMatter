var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add navigation state service
builder.Services.AddSingleton<Jackson.Ideas.Web.Services.NavigationState>();

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Configure routing - Map everything to /health endpoint for Render compatibility
app.MapRazorPages();
app.MapBlazorHub("/health/blazorhub");

// Map Blazor application to /health route (only working route on Render)
app.MapFallbackToPage("/health", "/_Host");

// Root redirect to /health for user convenience  
app.MapGet("/", () => Results.Redirect("/health"));

app.Run();
