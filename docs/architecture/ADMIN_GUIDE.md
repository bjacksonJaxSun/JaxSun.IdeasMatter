# System Administrator Guide - Ideas Matter

## 🔐 Administrator Access

### How to Login as System Administrator

To access the system administration panel, login with an email containing "admin@" or "systemadmin@":

**Demo System Admin Accounts:**
- `admin@example.com` / any password
- `systemadmin@ideasmatter.com` / any password
- Any email with "admin@" or "systemadmin@" in it

### Navigation
After login, system administrators are automatically redirected to `/admin`

## 🎛️ Admin Dashboard Features

### 1. Overview Tab
- **Total Tenants**: Number of organizations using the platform
- **Total Users**: Sum of all users across all tenants  
- **AI Providers**: Number of configured AI providers
- **Active Providers**: Providers with successful connections
- **Recent Activity**: System-wide activity log

### 2. AI Providers Tab

#### Supported AI Providers:
- **OpenAI** - GPT-4, GPT-3.5-turbo, etc.
- **Azure OpenAI** - Microsoft's OpenAI service
- **Anthropic Claude** - Claude 3 Opus, Sonnet, Haiku
- **Google Gemini** - Gemini Pro, Ultra
- **Meta Llama** - Llama 2, Code Llama
- **Custom Provider** - Any API-compatible service

#### Provider Configuration:
Each provider can be configured with:
- **API Key**: Authentication credential
- **Endpoint**: Custom API endpoint (for Azure/custom)
- **Model**: Specific model to use
- **Temperature**: Response creativity (0.0-1.0)
- **Max Tokens**: Maximum response length
- **Region**: Geographic region (for Azure)
- **Version**: API version (for Azure)

#### Provider Actions:
- **Test Connection**: Verify API connectivity
- **Enable/Disable**: Toggle provider availability
- **Configure**: Edit provider settings
- **Delete**: Remove provider (affects assigned tenants)

### 3. Tenants Tab

#### Tenant Management:
- View all organizational tenants
- See user counts per tenant
- Monitor assigned AI providers
- Check subscription plans (Free, Pro, Enterprise)
- Manage tenant status (Active, Suspended)

#### Tenant Actions:
- **Assign AI Provider**: Choose which AI service each tenant uses
- **Change Plan**: Upgrade/downgrade subscription
- **Suspend/Activate**: Control tenant access
- **View Details**: Deep dive into tenant metrics

### 4. Settings Tab
- Global system configuration
- Default AI provider settings
- Rate limiting and quotas
- Security policies

## 🔧 Common Administrative Tasks

### Adding a New AI Provider

1. Go to **AI Providers** tab
2. Click **"Add Provider"**
3. Select provider type:
   - OpenAI: Requires API key
   - Azure OpenAI: Requires API key, endpoint, deployment name
   - Claude: Requires API key
   - Gemini: Requires API key
   - Custom: Define your own API endpoints

4. Configure settings:
   ```
   Provider Name: OpenAI GPT-4 Production
   API Key: sk-proj-your-key-here
   Model: gpt-4-turbo-preview
   Temperature: 0.7
   Max Tokens: 4000
   ```

5. **Test Connection** to verify
6. **Enable** for tenant assignment

### Assigning AI Providers to Tenants

1. Go to **Tenants** tab
2. Find the tenant to configure
3. Click **"Manage"**
4. Select AI provider from dropdown
5. Save changes

### Monitoring AI Provider Health

- **Green Status**: Provider is active and responding
- **Red Status**: Provider has connection errors
- **Gray Status**: Provider is disabled

Use **"Test Connection"** to diagnose issues.

### Managing Tenant Limits

Based on subscription plans:
- **Free**: 100 ideas/month, basic AI models
- **Pro**: 1000 ideas/month, premium AI models
- **Enterprise**: Unlimited, custom AI models, priority support

## 🚨 Troubleshooting

### AI Provider Issues

**Provider shows "Error" status:**
1. Check API key validity
2. Verify endpoint URL (for Azure/custom)
3. Test with provider's documentation
4. Check rate limits/quotas
5. Verify network connectivity

**Provider test fails:**
1. Ensure API key has correct permissions
2. Check if service is experiencing outages
3. Verify model name is correct
4. Check region restrictions

### Tenant Issues

**Tenant can't access AI features:**
1. Verify tenant has active AI provider assigned
2. Check if provider is enabled and working
3. Verify tenant subscription hasn't expired
4. Check tenant status (not suspended)

**High usage alerts:**
1. Review tenant's idea generation patterns
2. Check for API abuse or automation
3. Consider plan upgrade or rate limiting
4. Monitor provider costs

## 🔒 Security Considerations

### API Key Management
- Store API keys securely (encrypted at rest)
- Rotate keys regularly
- Use environment variables, not config files
- Audit key usage and access

### Multi-Tenant Isolation
- Each tenant's data is isolated
- AI requests are attributed correctly
- No cross-tenant data leakage
- Separate billing/usage tracking

### Access Control
- System admin access is restricted
- Regular admins can only manage their tenant
- Audit logs track all admin actions
- Two-factor authentication recommended

## 📊 Monitoring & Analytics

### Key Metrics to Track:
- AI provider response times
- Token usage per tenant
- Error rates by provider
- User adoption rates
- Cost per tenant/request

### Alerts to Configure:
- Provider downtime
- High error rates
- Unusual usage spikes
- API quota approaching limits
- Failed authentication attempts

## 🔄 Backup & Recovery

### Critical Data:
- AI provider configurations
- Tenant settings and assignments
- User data and generated ideas
- Usage analytics and billing data

### Recovery Procedures:
1. Provider outage: Switch tenants to backup provider
2. Data corruption: Restore from latest backup
3. Security breach: Rotate all API keys, audit access
4. Service degradation: Enable rate limiting, notify users

---

This guide covers the essential system administration tasks for the Ideas Matter platform. For advanced configuration or custom integrations, consult the technical documentation or contact support.