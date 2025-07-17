var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Test if other common paths work like /health
app.MapGet("/status", () => "Status works!");
app.MapGet("/ping", () => "Ping works!");  
app.MapGet("/ready", () => "Ready works!");
app.MapGet("/live", () => "Live works!");
app.MapGet("/health", () => "Health still works!");

app.Run();
