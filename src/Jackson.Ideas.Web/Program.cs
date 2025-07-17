var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Minimal routing test - strip everything else
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapGet("/test", () => "Test endpoint works!");
app.MapGet("/", () => "Root endpoint works!");

app.Run();
