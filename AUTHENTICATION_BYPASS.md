# Authentication Bypass for Development

## Current Configuration

The application is currently configured to bypass authentication using a mock authentication service. This allows you to test the application without dealing with login/registration.

## Mock User Details

When using mock authentication, you're automatically logged in as:
- **Name**: Demo User
- **Email**: demo@ideasmatter.com
- **Role**: User
- **ID**: demo-user-123

## How It Works

1. **MockAuthenticationService**: Always returns successful authentication responses
2. **MockAuthenticationStateProvider**: Always provides an authenticated user state
3. **No Login Required**: All authenticated features are immediately accessible
4. **Navigation**: Click any button on the home page to go directly to the dashboard

## Configuration

The authentication mode is controlled by the `UseMockAuthentication` setting in `appsettings.json`:

```json
{
  "UseMockAuthentication": true  // Set to false to use real authentication
}
```

## Switching to Real Authentication

To enable real authentication:

1. Edit `src/Jackson.Ideas.Web/appsettings.json`
2. Set `"UseMockAuthentication": false`
3. Restart the application
4. Users will need to register/login with real credentials

## Features Available in Mock Mode

- ✅ Dashboard access
- ✅ New idea submission
- ✅ Research strategy selection
- ✅ Progress tracking
- ✅ Results viewing
- ✅ Profile page
- ❌ Admin features (requires Admin role)

## API Compatibility

The mock authentication only affects the Blazor frontend. The API still requires proper authentication tokens. In demo mode, the API services use mock implementations that don't require real authentication.

## Troubleshooting

### If you see "Failed to load resource" or redirect to /Account/Login

1. Ensure `UseMockAuthentication` is set to `true` in `appsettings.json`
2. Restart the application after changing the configuration
3. Clear your browser cache and cookies
4. Try accessing the site in an incognito/private browser window

### Common Issues

- **404 errors for /Account/Login**: This means mock authentication isn't properly configured
- **Infinite redirects**: Clear browser cookies and ensure the configuration is correct
- **"Not authorized" messages**: The mock authentication should bypass all authorization checks

## Production Deployment

**IMPORTANT**: Always set `UseMockAuthentication` to `false` in production environments to ensure proper security.

---

This bypass was implemented to facilitate UX testing and development without authentication friction. The real authentication system remains intact and can be re-enabled at any time.