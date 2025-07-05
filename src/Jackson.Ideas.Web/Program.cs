using Jackson.Ideas.Web.Components;
using Jackson.Ideas.Web.Services;
using Jackson.Ideas.Application.Extensions;
using Jackson.Ideas.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpContextAccessor for JWT authentication
builder.Services.AddHttpContextAccessor();

// Add HttpClient for API calls
builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001");
});
builder.Services.AddHttpClient<IAdminService, AdminService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001");
});

// Add Entity Framework context
builder.Services.AddDbContext<JacksonIdeasDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=jackson_ideas.db"));

// Add application services
builder.Services.AddApplicationServices();

// Add authentication services - using mock for now
var useMockAuth = builder.Configuration.GetValue<bool>("UseMockAuthentication", true);

if (useMockAuth)
{
    // Use mock authentication (bypasses login)
    builder.Services.AddScoped<IAuthenticationService, MockAuthenticationService>();
    builder.Services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();
    builder.Services.AddScoped<IAdminService, AdminService>();
}
else
{
    // Use real JWT authentication
    builder.Services.AddScoped<JwtAuthenticationStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
        provider.GetRequiredService<JwtAuthenticationStateProvider>());
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
}

// Add authentication and authorization
// Always add authentication services (required by the framework)
builder.Services.AddAuthentication(options =>
{
    if (useMockAuth)
    {
        // Set a default scheme for mock auth
        options.DefaultAuthenticateScheme = "MockScheme";
        options.DefaultChallengeScheme = "MockScheme";
    }
})
.AddScheme<MockAuthenticationSchemeOptions, MockAuthenticationHandler>("MockScheme", null)
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    if (useMockAuth)
    {
        // With mock auth, all policies pass
        options.AddPolicy("AdminOnly", policy => policy.RequireAssertion(_ => true));
        options.AddPolicy("UserOnly", policy => policy.RequireAssertion(_ => true));
    }
    else
    {
        // Real authorization policies
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin", "SystemAdmin"));
        options.AddPolicy("UserOnly", policy =>
            policy.RequireAuthenticatedUser());
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
