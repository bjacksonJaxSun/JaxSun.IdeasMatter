# Google Cloud Deployment Guide for Jackson Ideas

This guide walks you through deploying the Jackson Ideas platform to Google Cloud Platform (GCP) using Cloud Run.

## 📋 Prerequisites

1. **Google Cloud Account**
   - Create an account at https://cloud.google.com
   - Enable billing (required for Cloud Run)

2. **Google Cloud CLI**
   - Install from https://cloud.google.com/sdk/docs/install
   - Run `gcloud init` to authenticate

3. **Docker**
   - Install Docker Desktop from https://www.docker.com/products/docker-desktop

4. **.NET 8.0 SDK**
   - Already installed since you're developing the application

## 🚀 Quick Deploy

We've created an automated deployment script:

```bash
./deploy-to-gcp.sh
```

This script will:
- Enable required Google Cloud APIs
- Create necessary secrets
- Build Docker images
- Deploy to Cloud Run
- Configure the services

## 📝 Manual Deployment Steps

If you prefer manual deployment or need to customize:

### 1. Set up Google Cloud Project

```bash
# Set your project ID
export GCP_PROJECT_ID=your-project-id

# Set the project
gcloud config set project $GCP_PROJECT_ID

# Enable required APIs
gcloud services enable run.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable secretmanager.googleapis.com
```

### 2. Create Secrets

```bash
# Generate and store JWT secret
openssl rand -base64 32 | gcloud secrets create jackson-ideas-jwt-secret --data-file=-

# Create database connection secret
echo -n "your-connection-string" | gcloud secrets create jackson-ideas-db-connection --data-file=-
```

### 3. Build and Push Docker Images

```bash
# Build API image
docker build -t gcr.io/$GCP_PROJECT_ID/jackson-ideas-api:latest -f Dockerfile.api .
docker push gcr.io/$GCP_PROJECT_ID/jackson-ideas-api:latest

# Build Web image
docker build -t gcr.io/$GCP_PROJECT_ID/jackson-ideas-web:latest -f Dockerfile.web .
docker push gcr.io/$GCP_PROJECT_ID/jackson-ideas-web:latest
```

### 4. Deploy to Cloud Run

Deploy API:
```bash
gcloud run deploy jackson-ideas-api \
  --image gcr.io/$GCP_PROJECT_ID/jackson-ideas-api:latest \
  --region us-central1 \
  --allow-unauthenticated \
  --port 8080 \
  --memory 512Mi \
  --set-secrets "ConnectionStrings__DefaultConnection=jackson-ideas-db-connection:latest" \
  --set-secrets "JwtSettings__SecretKey=jackson-ideas-jwt-secret:latest"
```

Deploy Web:
```bash
# Get the API URL first
API_URL=$(gcloud run services describe jackson-ideas-api --region us-central1 --format='value(status.url)')

# Deploy Web with API URL
gcloud run deploy jackson-ideas-web \
  --image gcr.io/$GCP_PROJECT_ID/jackson-ideas-web:latest \
  --region us-central1 \
  --allow-unauthenticated \
  --port 8080 \
  --memory 512Mi \
  --set-env-vars "ApiSettings__BaseUrl=$API_URL"
```

## 🗄️ Database Options

### Option 1: Cloud SQL (Recommended for Production)

```bash
# Create PostgreSQL instance
gcloud sql instances create jackson-ideas-db \
  --tier=db-f1-micro \
  --region=us-central1 \
  --database-version=POSTGRES_15

# Create database
gcloud sql databases create jackson_ideas --instance=jackson-ideas-db

# Set password
gcloud sql users set-password postgres \
  --instance=jackson-ideas-db \
  --password=your-secure-password
```

Then update your Cloud Run service to connect:
```bash
gcloud run services update jackson-ideas-api \
  --add-cloudsql-instances=PROJECT_ID:REGION:jackson-ideas-db
```

### Option 2: Cloud Firestore (NoSQL Alternative)

Configure Firestore in your application and update the connection settings.

## 🔑 AI Provider Configuration

Add your AI provider API keys:

```bash
gcloud run services update jackson-ideas-api \
  --region us-central1 \
  --update-env-vars "DemoMode__ApiKeys__OpenAI=sk-..." \
  --update-env-vars "DemoMode__ApiKeys__Anthropic=sk-ant-..." \
  --update-env-vars "DemoMode__ApiKeys__Gemini=..."
```

## 🌐 Custom Domain Setup

1. Verify your domain in Google Cloud Console
2. Map your domain to Cloud Run:

```bash
gcloud run domain-mappings create \
  --service jackson-ideas-web \
  --domain your-domain.com \
  --region us-central1
```

3. Update DNS records as instructed

## 📊 Monitoring and Logging

View logs:
```bash
# API logs
gcloud run services logs read jackson-ideas-api --region us-central1

# Web logs
gcloud run services logs read jackson-ideas-web --region us-central1
```

Set up monitoring:
- Go to Cloud Console > Monitoring
- Create alerts for error rates, latency, etc.

## 💰 Cost Optimization

- **Cloud Run**: Pay only for what you use
- **Minimum instances**: Set to 0 for development
- **Maximum instances**: Limit to control costs
- **Memory allocation**: Start with 512Mi, increase if needed

## 🔒 Security Best Practices

1. **Enable Cloud Armor** for DDoS protection
2. **Use Secret Manager** for all sensitive data
3. **Enable VPC Service Controls** for network security
4. **Regular security scans** with Cloud Security Scanner

## 🐛 Troubleshooting

### Service won't start
- Check logs: `gcloud run services logs read SERVICE_NAME`
- Verify environment variables and secrets
- Ensure Docker image builds correctly

### Database connection issues
- Verify Cloud SQL proxy is enabled
- Check connection string in secrets
- Ensure network connectivity

### Authentication failures
- Verify JWT secret is correctly set
- Check CORS configuration
- Ensure API URL is correctly configured in Web service

## 📁 Files Created

- `Dockerfile.api` - API container configuration
- `Dockerfile.web` - Web container configuration
- `cloudbuild.yaml` - Automated build configuration
- `.gcloudignore` - Files to exclude from deployment
- `deploy-to-gcp.sh` - Automated deployment script
- Production config files for both API and Web

## 🎯 Next Steps

1. Run `./deploy-to-gcp.sh` to deploy
2. Configure your AI provider API keys
3. Set up a custom domain (optional)
4. Configure monitoring and alerts
5. Test the application thoroughly

Your application will be available at the URLs provided by the deployment script!