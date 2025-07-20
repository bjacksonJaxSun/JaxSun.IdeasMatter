# Routing and Layout Architecture

## Overview
This document defines the routing structure and layout assignments for the Ideas Matter application to prevent configuration drift and ensure consistency.

## Layout Strategy

### 1. LandingLayout (`Layout.LandingLayout`)
**Purpose**: Full-screen marketing/landing page without navigation sidebar
**Used By**: 
- Home page (`/`)

**Characteristics**:
- No sidebar navigation
- Full viewport usage
- Marketing-focused design
- Clean, professional presentation

### 2. MainLayout (`Layout.MainLayout`) 
**Purpose**: Application pages with sidebar navigation
**Used By**:
- Dashboard (`/dashboard`) 
- New Idea (`/new-idea`)
- Session Details (`/session-details`)
- Profile (`/profile`)
- Login (`/login`)
- Register (`/register`)
- Admin pages (`/admin/*`)

**Characteristics**:
- Left sidebar with navigation menu
- Home, Dashboard, New Idea navigation links
- Application-focused layout
- Consistent navigation experience

## Route Definitions

| Route | Component | Layout | Purpose |
|-------|-----------|--------|---------|
| `/` | Home.razor | LandingLayout | Marketing landing page |
| `/dashboard` | Dashboard.razor | MainLayout | User dashboard with research sessions |
| `/new-idea` | NewIdea.razor | MainLayout | Create new research session |
| `/session-details` | SessionDetails.razor | MainLayout | View research session details |
| `/profile` | Profile.razor | MainLayout | User profile management |
| `/login` | Login.razor | MainLayout | User authentication |
| `/register` | Register.razor | MainLayout | User registration |
| `/admin/users` | UserManagement.razor | MainLayout | Admin user management |
| `/admin/settings` | SystemSettings.razor | MainLayout | Admin system settings |

## Required Page Directives

### Landing Pages (LandingLayout)
```razor
@page "/"
@layout Layout.LandingLayout
```

### Application Pages (MainLayout)
```razor
@page "/dashboard"
@rendermode InteractiveServer
```
*Note: MainLayout is default, no explicit @layout directive needed*

## Navigation Menu Configuration

The navigation menu is defined in `Components/Layout/NavMenu.razor`:

```razor
<NavLink class="nav-link" href="" Match="NavLinkMatch.All">Home</NavLink>
<NavLink class="nav-link" href="dashboard">Dashboard</NavLink>
<NavLink class="nav-link" href="new-idea">New Idea</NavLink>
```

## Service Dependencies

### Required Service Registrations
These services must be registered in `Program.cs` for pages to function:

```csharp
// Navigation state management
builder.Services.AddScoped<Jackson.Ideas.Web.Services.NavigationState>();

// Authentication
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// API Services (conditional on UseMockAuthentication setting)
builder.Services.AddScoped<IAuthenticationService, [Mock]AuthenticationService>();
builder.Services.AddScoped<IResearchSessionApiService, [Mock]ResearchSessionApiService>();
builder.Services.AddScoped<IAdminService, [Mock]AdminService>();
```

### Common Page Dependencies
Most application pages inject these services:
- `NavigationState` - For programmatic navigation
- `IAuthenticationService` - For user authentication
- `IResearchSessionApiService` - For research data
- `HttpClient` - For API calls

## Layout Files Location
- `Components/Layout/LandingLayout.razor` - Landing page layout
- `Components/Layout/MainLayout.razor` - Application layout
- `Components/Layout/NavMenu.razor` - Navigation menu

## Styling Files
- `Components/Layout/MainLayout.razor.css` - Application layout styles
- `Components/Layout/NavMenu.razor.css` - Navigation styles

## Testing Checklist

When adding new pages, verify:
- [ ] Correct `@page` directive
- [ ] Appropriate layout assignment
- [ ] Required service injections
- [ ] Navigation menu updated (if needed)
- [ ] Build succeeds (`dotnet build`)
- [ ] Page loads without errors
- [ ] Navigation works correctly

## Common Issues & Solutions

### Issue: "Page not found" errors
**Solution**: Ensure `@page` directive is present and route is unique

### Issue: "Cannot provide a value for property" DI errors  
**Solution**: Verify service is registered in `Program.cs`

### Issue: Layout not applied correctly
**Solution**: Check `@layout` directive or verify default layout in `Routes.razor`

### Issue: Navigation links not working
**Solution**: Verify route matches exactly with `@page` directive

## Maintenance Guidelines

1. **Always document new routes** in this file
2. **Update navigation menu** when adding user-facing pages  
3. **Follow layout patterns** established here
4. **Test routing** after any changes
5. **Apply Golden Rule** - ensure clean build before completing changes

## Golden Rule Compliance

Before completing any routing/layout changes:
1. Run `dotnet build Jackson.Ideas.sln`
2. Fix any compilation errors
3. Verify all pages load correctly
4. Test navigation between pages
5. Only mark changes complete after clean build

This ensures routing integrity and prevents regression issues.