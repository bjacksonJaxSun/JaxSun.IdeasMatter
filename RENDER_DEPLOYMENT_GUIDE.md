# Render Deployment Guide for Jackson Ideas API

This guide will help you deploy the Jackson Ideas API to Render.com, a modern cloud platform that offers free hosting for web services.

## 📋 Prerequisites

1. **GitHub Account** - Your code needs to be in a GitHub repository
2. **Render Account** - Sign up for free at https://render.com

## 🚀 Quick Deploy Steps

### Step 1: Push Code to GitHub

First, you need to push your code to GitHub:

```bash
# Initialize git if not already done
git init

# Add all files
git add .

# Commit changes
git commit -m "Initial commit for Jackson Ideas platform"

# Create a new repository on GitHub, then:
git remote add origin https://github.com/YOUR_USERNAME/jackson-ideas.git
git branch -M main
git push -u origin main
```

### Step 2: Deploy to Render

1. **Go to Render Dashboard**
   - Visit https://dashboard.render.com
   - Click "New +" → "Web Service"

2. **Connect GitHub Repository**
   - Click "Connect GitHub"
   - Authorize Render to access your repositories
   - Select your `jackson-ideas` repository

3. **Configure the Service**
   - **Name**: `jackson-ideas-api`
   - **Region**: Select closest to your users
   - **Branch**: `main`
   - **Runtime**: `Docker`
   - **Dockerfile Path**: `./Dockerfile.api.render`
   - **Docker Context Directory**: `.`

4. **Set Environment Variables**
   Click "Advanced" and add these environment variables:
   ```
   ASPNETCORE_ENVIRONMENT = Production
   ASPNETCORE_URLS = http://+:8080
   ConnectionStrings__DefaultConnection = Data Source=/app/jackson_ideas.db
   Jwt__SecretKey = [Generate a secure key]
   Jwt__Issuer = https://jackson-ideas-api.onrender.com
   Jwt__Audience = https://jackson-ideas-web.run.app
   ```

5. **Create Web Service**
   - Click "Create Web Service"
   - Wait for the build and deployment to complete (5-10 minutes)

### Step 3: Update Your Web Application

Once your API is deployed on Render, you'll get a URL like:
`https://jackson-ideas-api.onrender.com`

Update your Google Cloud Web application to use this API:

```bash
gcloud run services update jackson-ideas-web \
  --region=us-central1 \
  --update-env-vars="ApiSettings__BaseUrl=https://jackson-ideas-api.onrender.com"
```

## 🎯 Alternative: One-Click Deploy

You can use the render.yaml file for automated deployment:

1. Push the `render.yaml` file to your GitHub repository
2. In Render Dashboard, click "New +" → "Blueprint"
3. Connect your repository
4. Render will automatically detect the render.yaml and set everything up

## 🔧 Manual Git Commands (if needed)

If you haven't initialized git yet:

```bash
# Initialize git
git init

# Configure git (if not already done)
git config --global user.email "bjackson071968@gmail.com"
git config --global user.name "Your Name"

# Add files
git add .

# Commit
git commit -m "Add Render deployment configuration"

# Add remote (replace with your repository URL)
git remote add origin https://github.com/YOUR_USERNAME/jackson-ideas.git

# Push to GitHub
git push -u origin main
```

## 📊 Monitoring Your Deployment

1. **View Logs**: In Render Dashboard → Your Service → "Logs"
2. **Check Health**: Visit `https://your-api-url.onrender.com/health`
3. **API Documentation**: Visit `https://your-api-url.onrender.com/swagger`

## 💰 Pricing

- **Free Tier**: 750 hours/month (perfect for testing)
- **Paid Plans**: Starting at $7/month for always-on service
- **Free tier limitations**: Service spins down after 15 minutes of inactivity

## 🎉 Success Checklist

- [ ] Code pushed to GitHub
- [ ] Render service created
- [ ] API deployed and accessible
- [ ] Health endpoint responding
- [ ] Web app updated with new API URL
- [ ] Full application working end-to-end

## 🆘 Troubleshooting

**Build Fails**
- Check Dockerfile.api.render exists
- Ensure all project files are committed
- Review build logs in Render dashboard

**Service Won't Start**
- Check environment variables are set correctly
- Verify PORT is set to 8080
- Check application logs for errors

**Database Issues**
- SQLite file is created automatically
- For production, consider PostgreSQL (Render offers free PostgreSQL)

## 🎯 Next Steps

1. Visit your complete application:
   - Web: https://jackson-ideas-web-480585172218.us-central1.run.app
   - API: https://your-api-url.onrender.com

2. Test the full workflow:
   - Register a new user
   - Login
   - Submit an idea for analysis

3. Configure AI providers:
   - Add OpenAI/Anthropic API keys in Render environment variables

Your Jackson Ideas platform is now fully deployed with the Web app on Google Cloud and the API on Render!