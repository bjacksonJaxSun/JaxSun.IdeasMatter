var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Test route ordering hypothesis - put /test first
app.MapGet("/test", () => "Test endpoint works!");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapGet("/", () => "Root endpoint works!");

app.Run();
