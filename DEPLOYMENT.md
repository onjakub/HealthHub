# HealthHub - Deployment Guide

## Overview

This document describes various deployment methods for the HealthHub application, from local development to production environments.

## Development Deployment

### Local Development with Docker Compose (recommended)

#### 1. Environment Preparation

```bash
# Clone the repository
git clone <repository-url>
cd HealthHub

# Check Docker Compose configuration
docker-compose config
```

#### 2. Start the Application

```bash
# Start all services
docker-compose up -d

# Check service status
docker-compose ps

# View logs
docker-compose logs -f healthhub-api
```

#### 3. Deployment Verification

- **Frontend**: http://localhost:3000
- **GraphQL API**: http://localhost:5023/graphql
- **Health Check**: http://localhost:5023/health
- **Database**: PostgreSQL on localhost:5432

### Local Development without Docker

#### 1. Database Setup

```bash
# Start PostgreSQL in Docker
docker-compose up healthhub-db -d

# Or local PostgreSQL instance
# Ensure connection string matches your setup
```

#### 2. Start the Backend

```bash
cd HealthHub

# Restore dependencies
dotnet restore

# Start the application
dotnet run
```

Backend runs on: **http://localhost:5023**

#### 3. Start the Frontend

```bash
cd Frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

Frontend runs on: **http://localhost:3000**

## Production Deployment

### Docker Production Deployment

#### 1. Build Docker Image

```bash
# Build image with production configuration
docker build -t healthhub:latest -f HealthHub/Dockerfile .

# Verify image
docker images | grep healthhub
```

#### 2. Environment Configuration

Create `docker-compose.prod.yml` for production:

```yaml
services:
  healthhub-db:
    image: postgres:16
    environment:
      POSTGRES_DB: healthhub
      POSTGRES_USER: health
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - healthhub_data:/var/lib/postgresql/data

  healthhub-api:
    image: healthhub:latest
    environment:
      DB_CONNECTION: "Host=healthhub-db;Database=healthhub;Username=health;Password=${DB_PASSWORD}"
      JWT_KEY: ${JWT_SECRET}
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "80:8080"
    depends_on:
      - healthhub-db
```

#### 3. Start Production Stack

```bash
# Set environment variables
export DB_PASSWORD="secure-production-password"
export JWT_SECRET="very-long-secure-jwt-secret-key"

# Start production stack
docker-compose -f docker-compose.prod.yml up -d
```

### Manual Production Deployment

#### 1. Build Process

```bash
# Build frontend
cd Frontend
npm install
npm run build

# Build backend
cd ../HealthHub
dotnet publish -c Release -o ./publish
```

#### 2. Server Deployment

```bash
# Copy to server
scp -r ./publish user@server:/opt/healthhub/

# Set permissions
ssh user@server "chmod +x /opt/healthhub/HealthHub.dll"

# Run as service
ssh user@server "cd /opt/healthhub && dotnet HealthHub.dll"
```

#### 3. Systemd Service (Linux)

Create `/etc/systemd/system/healthhub.service`:

```ini
[Unit]
Description=HealthHub Patient Management System
After=network.target

[Service]
Type=exec
WorkingDirectory=/opt/healthhub
ExecStart=/usr/bin/dotnet /opt/healthhub/HealthHub.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DB_CONNECTION=Host=localhost;Database=healthhub;Username=health;Password=secure-password
Environment=JWT_KEY=very-long-secure-jwt-secret-key

[Install]
WantedBy=multi-user.target
```

Start the service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable healthhub
sudo systemctl start healthhub
sudo systemctl status healthhub
```

## Cloud Deployment

### Azure App Service Deployment

#### 1. Azure CLI Setup

```bash
# Login to Azure
az login

# Create resource group
az group create --name healthhub-rg --location "West Europe"

# Create App Service plan
az appservice plan create --name healthhub-plan --resource-group healthhub-rg --sku B1 --is-linux

# Create web app
az webapp create --resource-group healthhub-rg --plan healthhub-plan --name healthhub-app --runtime "DOTNETCORE:10.0"
```

#### 2. Environment Configuration

```bash
# Set environment variables
az webapp config appsettings set --resource-group healthhub-rg --name healthhub-app --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  DB_CONNECTION="Server=your-server.database.windows.net;Database=healthhub;User Id=health;Password=your-password;" \
  JWT_KEY="your-jwt-secret"
```

#### 3. Deployment

```bash
# ZIP deployment
zip -r healthhub.zip . -x "node_modules/*" ".git/*"
az webapp deployment source config-zip --resource-group healthhub-rg --name healthhub-app --src healthhub.zip
```

### AWS Elastic Beanstalk Deployment

#### 1. EB CLI Setup

```bash
# Install EB CLI
pip install awsebcli

# Initialize EB application
eb init -p docker healthhub-app
```

#### 2. Configuration

Create `.ebextensions/healthhub.config`:

```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    DB_CONNECTION: postgresql://health:password@host:5432/healthhub
    JWT_KEY: your-jwt-secret
```

#### 3. Deployment

```bash
# Create environment
eb create healthhub-env

# Deploy
eb deploy
```

## Database Deployment

### PostgreSQL Setup

#### 1. Local PostgreSQL

```bash
# Install PostgreSQL
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib

# Create database
sudo -u postgres psql -c "CREATE DATABASE healthhub;"
sudo -u postgres psql -c "CREATE USER health WITH PASSWORD 'healthpwd';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE healthhub TO health;"
```

#### 2. Cloud PostgreSQL (Azure)

```bash
# Create PostgreSQL server
az postgres server create \
  --resource-group healthhub-rg \
  --name healthhub-pg \
  --location "West Europe" \
  --admin-user health \
  --admin-password "secure-password" \
  --sku-name B_Gen5_1 \
  --version 11

# Create database
az postgres db create \
  --resource-group healthhub-rg \
  --server-name healthhub-pg \
  --name healthhub
```

### Database Migrations

```bash
# EF Core migrations
cd HealthHub
dotnet ef migrations add InitialCreate
dotnet ef database update

# Or automatic database creation
# Application automatically creates database on startup using EnsureCreatedAsync()
```

## SSL/HTTPS Configuration

### Production SSL Setup

#### 1. Let's Encrypt Certificate

```bash
# Install certbot
sudo apt-get install certbot

# Obtain certificate
sudo certbot certonly --standalone -d your-domain.com

# Configuration in application
# Add to appsettings.Production.json
```

#### 2. Reverse Proxy (Nginx)

Create `/etc/nginx/sites-available/healthhub`:

```nginx
server {
    listen 80;
    server_name your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl;
    server_name your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Monitoring and Logging

### Application Insights (Azure)

Add to `HealthHub.csproj`:
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
```

Configuration in `Program.cs`:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Custom Logging

```bash
# View logs
docker-compose logs -f healthhub-api

# Production logs
journalctl -u healthhub.service -f

# Log rotation configuration
sudo logrotate -f /etc/logrotate.d/healthhub
```

## Backup and Recovery

### Database Backup

```bash
# PostgreSQL backup
pg_dump -h localhost -U health -d healthhub > healthhub_backup.sql

# Automatic backups (cron job)
0 2 * * * pg_dump -h localhost -U health -d healthhub > /backups/healthhub_$(date +\%Y\%m\%d).sql
```

### Application Backup

```bash
# Backup configuration and data
tar -czf healthhub_backup_$(date +%Y%m%d).tar.gz \
  /opt/healthhub \
  /etc/systemd/system/healthhub.service \
  /backups/
```

## Troubleshooting Deployment

### Common Issues

#### Port Conflicts
```bash
# Find processes on port
lsof -i :8080
# Kill process
kill -9 <PID>
```

#### Database Connection Issues
```bash
# Test database connection
psql -h localhost -U health -d healthhub

# Check PostgreSQL service
sudo systemctl status postgresql
```

#### Build Issues
```bash
# Clean build
cd Frontend
npm run clean:all
npm install
npm run build

cd ../HealthHub
dotnet clean
dotnet build
```

### Health Checks

```bash
# Test health endpoint
curl -f http://localhost:8080/health

# Test GraphQL API
curl -X POST http://localhost:8080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"query { __typename }"}'
```

## Performance Optimization

### Production Optimizations

1. **Enable Gzip Compression** - in `Program.cs`
2. **Configure Caching** - static files caching
3. **Database Connection Pooling** - connection string optimization
4. **Frontend Optimization** - code splitting and minification

### Monitoring Performance

```bash
# Resource usage
docker stats healthhub-api

# Database performance
pg_stat_statements extension
```

---

This deployment guide covers all common deployment scenarios for the HealthHub application from development to production environments.