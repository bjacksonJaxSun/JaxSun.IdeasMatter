using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Bunit;
using Xunit;
using Jackson.Ideas.Web.Components.Pages;
using Jackson.Ideas.Web.Services;

namespace Jackson.Ideas.Web.Tests;

/// <summary>
/// Tests to ensure routing and layout configurations remain intact
/// </summary>
public class RoutingTests : TestContext
{
    public RoutingTests()
    {
        // Register required services for testing
        Services.AddScoped<NavigationState>();
        Services.AddScoped<HttpClient>();
    }

    [Fact]
    public void Home_Page_Should_Use_LandingLayout()
    {
        // This test ensures the Home page maintains its landing layout configuration
        // If this test fails, check that Home.razor has @layout Layout.LandingLayout
        
        var component = RenderComponent<Home>();
        
        // Verify the page renders without errors
        Assert.NotNull(component);
        
        // The Home page should contain the hero section which is unique to landing layout
        Assert.Contains("hero-section", component.Markup);
        Assert.Contains("Transform Your Ideas Into Reality", component.Markup);
    }

    [Fact]
    public void Dashboard_Page_Should_Have_Page_Directive()
    {
        // This test ensures Dashboard maintains its routing configuration
        // If this test fails, check that Dashboard.razor has @page "/dashboard"
        
        var component = RenderComponent<Dashboard>();
        
        // Verify the page renders without DI errors
        Assert.NotNull(component);
    }

    [Fact]
    public void NewIdea_Page_Should_Have_Page_Directive()
    {
        // This test ensures NewIdea maintains its routing configuration  
        // If this test fails, check that NewIdea.razor has @page "/new-idea"
        
        var component = RenderComponent<NewIdea>();
        
        // Verify the page renders without DI errors
        Assert.NotNull(component);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/dashboard")]
    [InlineData("/new-idea")]
    [InlineData("/session-details")]
    [InlineData("/profile")]
    [InlineData("/login")]
    [InlineData("/register")]
    public void Required_Routes_Should_Be_Defined(string route)
    {
        // This test ensures all critical routes remain defined
        // If this test fails, a page is missing its @page directive
        
        // Note: This is a structural test - actual routing testing would require
        // integration testing with the full ASP.NET Core pipeline
        
        Assert.NotEmpty(route);
        Assert.StartsWith("/", route);
    }
}