var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Test what makes /health special
app.MapGet("/test", () => Results.Ok(new { status = "test works", timestamp = DateTime.UtcNow }));
app.MapGet("/health2", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));  
app.MapGet("/health", () => "Health with string return!");
app.MapGet("/simple", () => "Simple string");

app.Run();
