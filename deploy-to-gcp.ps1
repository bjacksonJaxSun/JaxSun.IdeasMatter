# Google Cloud Deployment Script for Jackson Ideas Platform (Windows PowerShell)
# This script deploys both the API and Web components to Google Cloud Run

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Jackson Ideas - Google Cloud Deployment" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Configuration
$PROJECT_ID = "ideas-matter-1749958193112"
$REGION = "us-central1"
$API_SERVICE_NAME = "jackson-ideas-api"
$WEB_SERVICE_NAME = "jackson-ideas-web"

Write-Host ""
Write-Host "Using Project ID: $PROJECT_ID" -ForegroundColor Green
Write-Host "Using Region: $REGION" -ForegroundColor Green
Write-Host ""

# Check if gcloud is installed
try {
    $gcloudVersion = gcloud --version 2>$null
    Write-Host "Google Cloud CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "Error: gcloud CLI is not installed. Please install it from:" -ForegroundColor Red
    Write-Host "https://cloud.google.com/sdk/docs/install" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "For Windows, download and run the installer from the link above." -ForegroundColor Yellow
    exit 1
}

# Set the project
Write-Host "Setting Google Cloud project..." -ForegroundColor Yellow
gcloud config set project $PROJECT_ID

# Enable required APIs
Write-Host ""
Write-Host "Enabling required Google Cloud APIs..." -ForegroundColor Yellow
gcloud services enable run.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable sqladmin.googleapis.com
gcloud services enable containerregistry.googleapis.com

# Create secrets if they don't exist
Write-Host ""
Write-Host "Setting up secrets in Secret Manager..." -ForegroundColor Yellow

# Check if JWT secret exists
$jwtSecretExists = gcloud secrets describe jackson-ideas-jwt-secret 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Creating JWT secret..." -ForegroundColor Yellow
    $JWT_SECRET = Read-Host "Enter JWT Secret Key (or press enter to generate)"
    if ([string]::IsNullOrWhiteSpace($JWT_SECRET)) {
        # Generate a random JWT secret
        Add-Type -AssemblyName System.Web
        $JWT_SECRET = [System.Web.Security.Membership]::GeneratePassword(32, 8)
        Write-Host "Generated JWT Secret: $JWT_SECRET" -ForegroundColor Green
    }
    $JWT_SECRET | gcloud secrets create jackson-ideas-jwt-secret --data-file=-
} else {
    Write-Host "JWT secret already exists" -ForegroundColor Green
}

# Database connection setup
Write-Host ""
Write-Host "Database Configuration:" -ForegroundColor Yellow
Write-Host "1. Use Cloud SQL (recommended for production)" -ForegroundColor White
Write-Host "2. Use SQLite (for testing only)" -ForegroundColor White
$DB_OPTION = Read-Host "Choose option (1-2)"

if ($DB_OPTION -eq "1") {
    # Cloud SQL setup
    Write-Host ""
    Write-Host "Setting up Cloud SQL..." -ForegroundColor Yellow
    $INSTANCE_NAME = "jackson-ideas-db"
    $DB_NAME = "jackson_ideas"
    
    # Check if instance exists
    $instanceExists = gcloud sql instances describe $INSTANCE_NAME 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Creating Cloud SQL instance (this may take several minutes)..." -ForegroundColor Yellow
        gcloud sql instances create $INSTANCE_NAME `
            --tier=db-f1-micro `
            --region=$REGION `
            --database-version=POSTGRES_15
        
        # Set password for postgres user
        $DB_PASSWORD = Read-Host "Enter password for postgres user" -AsSecureString
        $DB_PASSWORD_TEXT = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DB_PASSWORD))
        
        gcloud sql users set-password postgres `
            --instance=$INSTANCE_NAME `
            --password=$DB_PASSWORD_TEXT
        
        # Create database
        gcloud sql databases create $DB_NAME --instance=$INSTANCE_NAME
    } else {
        Write-Host "Cloud SQL instance already exists" -ForegroundColor Green
        $DB_PASSWORD = Read-Host "Enter password for postgres user" -AsSecureString
        $DB_PASSWORD_TEXT = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DB_PASSWORD))
    }
    
    # Create connection string secret
    $CONNECTION_STRING = "Host=/cloudsql/${PROJECT_ID}:${REGION}:${INSTANCE_NAME};Database=$DB_NAME;Username=postgres;Password=$DB_PASSWORD_TEXT"
    $CONNECTION_STRING | gcloud secrets create jackson-ideas-db-connection --data-file=- 2>$null
    if ($LASTEXITCODE -ne 0) {
        $CONNECTION_STRING | gcloud secrets versions add jackson-ideas-db-connection --data-file=-
    }
    
    $SQL_CONNECTION_ARG = "--add-cloudsql-instances=${PROJECT_ID}:${REGION}:${INSTANCE_NAME}"
} else {
    # SQLite setup
    Write-Host "Using SQLite (not recommended for production)" -ForegroundColor Yellow
    $CONNECTION_STRING = "Data Source=/app/jackson_ideas.db"
    $CONNECTION_STRING | gcloud secrets create jackson-ideas-db-connection --data-file=- 2>$null
    if ($LASTEXITCODE -ne 0) {
        $CONNECTION_STRING | gcloud secrets versions add jackson-ideas-db-connection --data-file=-
    }
    $SQL_CONNECTION_ARG = ""
}

# Build and deploy
Write-Host ""
Write-Host "Starting deployment to Google Cloud..." -ForegroundColor Yellow
Write-Host "This will use Cloud Build to build and deploy your application." -ForegroundColor White

# Submit build
gcloud builds submit --config cloudbuild.yaml .

# Wait for services to be deployed
Write-Host ""
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Get the service URLs
Write-Host ""
Write-Host "Getting service URLs..." -ForegroundColor Yellow
$API_URL = gcloud run services describe $API_SERVICE_NAME --region=$REGION --format='value(status.url)'
$WEB_URL = gcloud run services describe $WEB_SERVICE_NAME --region=$REGION --format='value(status.url)'

# Update Web service with correct API URL
Write-Host "Updating Web service with API URL..." -ForegroundColor Yellow
gcloud run services update $WEB_SERVICE_NAME `
    --region=$REGION `
    --update-env-vars="ApiSettings__BaseUrl=$API_URL"

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "API URL: $API_URL" -ForegroundColor Cyan
Write-Host "Web URL: $WEB_URL" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Visit $WEB_URL to access your application" -ForegroundColor White
Write-Host "2. Configure API keys for AI providers in the Google Cloud Console" -ForegroundColor White
Write-Host "3. Set up a custom domain (optional)" -ForegroundColor White
Write-Host "4. Configure monitoring and alerts" -ForegroundColor White
Write-Host ""
Write-Host "To add AI provider API keys:" -ForegroundColor Yellow
Write-Host "gcloud run services update $API_SERVICE_NAME --region=$REGION ``" -ForegroundColor Gray
Write-Host "  --update-env-vars=`"DemoMode__ApiKeys__OpenAI=your-key,DemoMode__ApiKeys__Anthropic=your-key`"" -ForegroundColor Gray
Write-Host "================================================" -ForegroundColor Green