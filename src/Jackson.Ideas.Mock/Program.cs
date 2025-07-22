using Jackson.Ideas.Mock.Services.Interfaces;
using Jackson.Ideas.Mock.Services.Mock;
using Jackson.Ideas.Mock.Services;
using Jackson.Ideas.Mock.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

// Configure Mock services
builder.Services.Configure<MockConfiguration>(builder.Configuration.GetSection("MockConfiguration"));

// Register Mock services
builder.Services.AddScoped<IMockDataService, MockDataService>();
builder.Services.AddScoped<IMockAuthenticationService, MockAuthenticationService>();
builder.Services.AddScoped<IMarketResearchService, MockMarketResearchService>();
builder.Services.AddScoped<IFinancialProjectionService, MockFinancialProjectionService>();
builder.Services.AddScoped<IUserProfileService, MockUserProfileService>();

// Register Business Translation Service
builder.Services.AddScoped<BusinessTranslationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();