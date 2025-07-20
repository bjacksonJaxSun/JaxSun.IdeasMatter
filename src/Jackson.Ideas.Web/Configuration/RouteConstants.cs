namespace Jackson.Ideas.Web.Configuration;

/// <summary>
/// Centralized route definitions to prevent routing inconsistencies
/// </summary>
public static class RouteConstants
{
    // Landing Pages
    public const string Home = "/";
    
    // Application Pages
    public const string Dashboard = "/dashboard";
    public const string NewIdea = "/new-idea";
    public const string SessionDetails = "/session-details";
    public const string Profile = "/profile";
    
    // Authentication Pages
    public const string Login = "/login";
    public const string Register = "/register";
    
    // Admin Pages
    public const string AdminUsers = "/admin/users";
    public const string AdminSettings = "/admin/settings";
    
    // API Routes (for reference)
    public const string ApiBase = "/api/v1";
    public const string ApiAuth = "/api/v1/auth";
    public const string ApiResearchSessions = "/api/v1/research/sessions";
}

/// <summary>
/// Layout assignments for pages
/// </summary>
public static class LayoutConstants
{
    public const string LandingLayout = "Layout.LandingLayout";
    public const string MainLayout = "Layout.MainLayout";
}

/// <summary>
/// Navigation menu configuration
/// </summary>
public static class NavigationConstants
{
    public static readonly (string Text, string Route, string Icon)[] MenuItems = 
    [
        ("Home", RouteConstants.Home, "fas fa-home"),
        ("Dashboard", RouteConstants.Dashboard, "fas fa-tachometer-alt"),
        ("New Idea", RouteConstants.NewIdea, "fas fa-plus-square")
    ];
}