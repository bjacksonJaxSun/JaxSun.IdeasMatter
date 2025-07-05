#!/bin/bash

# Google Cloud Deployment Script for Jackson Ideas Platform
# This script deploys both the API and Web components to Google Cloud Run

set -e

echo "================================================"
echo "Jackson Ideas - Google Cloud Deployment"
echo "================================================"

# Check if gcloud is installed
if ! command -v gcloud &> /dev/null; then
    echo "Error: gcloud CLI is not installed. Please install it from:"
    echo "https://cloud.google.com/sdk/docs/install"
    exit 1
fi

# Configuration
PROJECT_ID=${GCP_PROJECT_ID:-""}
REGION=${GCP_REGION:-"us-central1"}
API_SERVICE_NAME="jackson-ideas-api"
WEB_SERVICE_NAME="jackson-ideas-web"

# Check if PROJECT_ID is set
if [ -z "$PROJECT_ID" ]; then
    echo "Please set your Google Cloud Project ID:"
    echo "export GCP_PROJECT_ID=your-project-id"
    exit 1
fi

echo "Using Project ID: $PROJECT_ID"
echo "Using Region: $REGION"
echo ""

# Set the project
gcloud config set project $PROJECT_ID

# Enable required APIs
echo "Enabling required Google Cloud APIs..."
gcloud services enable run.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable sqladmin.googleapis.com

# Create secrets if they don't exist
echo ""
echo "Setting up secrets in Secret Manager..."

# Check if JWT secret exists
if ! gcloud secrets describe jackson-ideas-jwt-secret &> /dev/null; then
    echo "Creating JWT secret..."
    echo -n "Enter JWT Secret Key (or press enter to generate): "
    read JWT_SECRET
    if [ -z "$JWT_SECRET" ]; then
        JWT_SECRET=$(openssl rand -base64 32)
        echo "Generated JWT Secret: $JWT_SECRET"
    fi
    echo -n "$JWT_SECRET" | gcloud secrets create jackson-ideas-jwt-secret --data-file=-
else
    echo "JWT secret already exists"
fi

# Database connection setup
echo ""
echo "Database Configuration:"
echo "1. Use Cloud SQL (recommended for production)"
echo "2. Use SQLite (for testing only)"
echo -n "Choose option (1-2): "
read DB_OPTION

if [ "$DB_OPTION" = "1" ]; then
    # Cloud SQL setup
    echo ""
    echo "Setting up Cloud SQL..."
    INSTANCE_NAME="jackson-ideas-db"
    DB_NAME="jackson_ideas"
    
    # Check if instance exists
    if ! gcloud sql instances describe $INSTANCE_NAME &> /dev/null; then
        echo "Creating Cloud SQL instance..."
        gcloud sql instances create $INSTANCE_NAME \
            --tier=db-f1-micro \
            --region=$REGION \
            --database-version=POSTGRES_15
        
        # Set password for postgres user
        echo -n "Enter password for postgres user: "
        read -s DB_PASSWORD
        echo ""
        gcloud sql users set-password postgres \
            --instance=$INSTANCE_NAME \
            --password=$DB_PASSWORD
        
        # Create database
        gcloud sql databases create $DB_NAME --instance=$INSTANCE_NAME
    else
        echo "Cloud SQL instance already exists"
        echo -n "Enter password for postgres user: "
        read -s DB_PASSWORD
        echo ""
    fi
    
    # Create connection string secret
    CONNECTION_STRING="Host=/cloudsql/$PROJECT_ID:$REGION:$INSTANCE_NAME;Database=$DB_NAME;Username=postgres;Password=$DB_PASSWORD"
    echo -n "$CONNECTION_STRING" | gcloud secrets create jackson-ideas-db-connection --data-file=- || \
    echo -n "$CONNECTION_STRING" | gcloud secrets versions add jackson-ideas-db-connection --data-file=-
    
    # Update cloudbuild.yaml to include Cloud SQL proxy
    SQL_CONNECTION_ARG="--add-cloudsql-instances=$PROJECT_ID:$REGION:$INSTANCE_NAME"
else
    # SQLite setup (not recommended for production)
    echo "Using SQLite (not recommended for production)"
    CONNECTION_STRING="Data Source=/app/jackson_ideas.db"
    echo -n "$CONNECTION_STRING" | gcloud secrets create jackson-ideas-db-connection --data-file=- || \
    echo -n "$CONNECTION_STRING" | gcloud secrets versions add jackson-ideas-db-connection --data-file=-
    SQL_CONNECTION_ARG=""
fi

# Build and deploy using Cloud Build
echo ""
echo "Starting Cloud Build deployment..."

# Update cloudbuild.yaml with SQL connection if needed
if [ ! -z "$SQL_CONNECTION_ARG" ]; then
    # Create a temporary cloudbuild file with SQL connection
    cp cloudbuild.yaml cloudbuild.temp.yaml
    sed -i "s/--set-env-vars=/--set-env-vars=GOOGLE_CLOUD_PROJECT=$PROJECT_ID,/" cloudbuild.temp.yaml
    sed -i "/jackson-ideas-api/a\\      - '$SQL_CONNECTION_ARG'" cloudbuild.temp.yaml
    
    gcloud builds submit --config cloudbuild.temp.yaml .
    rm cloudbuild.temp.yaml
else
    gcloud builds submit --config cloudbuild.yaml .
fi

# Get the service URLs
echo ""
echo "Getting service URLs..."
API_URL=$(gcloud run services describe $API_SERVICE_NAME --region=$REGION --format='value(status.url)')
WEB_URL=$(gcloud run services describe $WEB_SERVICE_NAME --region=$REGION --format='value(status.url)')

# Update Web service with correct API URL
echo "Updating Web service with API URL..."
gcloud run services update $WEB_SERVICE_NAME \
    --region=$REGION \
    --update-env-vars="ApiSettings__BaseUrl=$API_URL"

echo ""
echo "================================================"
echo "Deployment Complete!"
echo "================================================"
echo "API URL: $API_URL"
echo "Web URL: $WEB_URL"
echo ""
echo "Next steps:"
echo "1. Visit $WEB_URL to access your application"
echo "2. Configure API keys for AI providers in the Google Cloud Console"
echo "3. Set up a custom domain (optional)"
echo "4. Configure monitoring and alerts"
echo ""
echo "To add AI provider API keys:"
echo "gcloud run services update $API_SERVICE_NAME --region=$REGION \\"
echo "  --update-env-vars=\"DemoMode__ApiKeys__OpenAI=your-key,DemoMode__ApiKeys__Anthropic=your-key\""
echo "================================================"