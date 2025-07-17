var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure static files
app.UseStaticFiles();

// Add routing
app.UseRouting();

// Map everything to /health since that's the only route Render allows
app.MapRazorPages();
app.MapBlazorHub("/health/blazorhub");
app.MapFallbackToPage("/health", "/_Host");

// Root redirect to /health
app.MapGet("/", () => Results.Redirect("/health"));

app.Run();
