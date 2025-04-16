# CineScope Complete IT Administrator Guide

## Table of Contents
1. [Introduction](#1-introduction)
2. [Initial Setup](#2-initial-setup)
   - [Development Environment](#2a-development-environment)
   - [Azure Deployment](#2b-azure-deployment)
   - [MongoDB Configuration](#2c-mongodb-configuration)
   - [SSL and Security Setup](#2d-ssl-and-security-setup)
3. [Monitoring and Maintenance](#3-monitoring-and-maintenance)
   - [Application Insights](#3a-application-insights)
   - [Performance Monitoring](#3b-performance-monitoring)
   - [Database Health](#3c-database-health)
   - [Log Analysis](#3d-log-analysis)
4. [Backup and Recovery](#4-backup-and-recovery)
   - [Automated Backups](#4a-automated-backups)
   - [Recovery Procedures](#4b-recovery-procedures)
   - [Disaster Recovery](#4c-disaster-recovery)
5. [Security Management](#5-security-management)
   - [Access Control](#5a-access-control)
   - [Threat Monitoring](#5b-threat-monitoring)
   - [Patch Management](#5c-patch-management)
6. [Performance Optimization](#6-performance-optimization)
   - [Caching System](#6a-caching-system)
   - [Database Optimization](#6b-database-optimization)
   - [Content Delivery](#6c-content-delivery)
7. [Content Management](#7-content-management)
   - [Movie Database](#7a-movie-database)
   - [Review System](#7b-review-system)
   - [User Management](#7c-user-management)
8. [Search and Filtering](#8-search-and-filtering)
   - [Search Configuration](#8a-search-configuration)
   - [Filter Management](#8b-filter-management)
   - [Performance Tuning](#8c-performance-tuning)
9. [System Updates](#9-system-updates)
   - [Deployment Process](#9a-deployment-process)
   - [Testing Protocol](#9b-testing-protocol)
   - [Rollback Procedures](#9c-rollback-procedures)
10. [Troubleshooting](#10-troubleshooting)
    - [Common Issues](#10a-common-issues)
    - [Error Reference](#10b-error-reference)
    - [Support Procedures](#10c-support-procedures)
11. [IT Staff Team Instructions](#11-it-staff-team-instructions)
    - [Roles and Responsibilities](#11a-roles-and-responsibilities)
    - [Daily Operations](#11b-daily-operations)
    - [Weekly Maintenance](#11c-weekly-maintenance)
    - [Monthly Tasks](#11d-monthly-tasks)
    - [Emergency Procedures](#11e-emergency-procedures)
    - [Escalation Path](#11f-escalation-path)

# CineScope Complete IT Administrator Guide

## 1. Introduction

CineScope is a movie review platform built on Azure using .NET 8.0 and MongoDB. This guide provides comprehensive instructions for deploying, maintaining, and troubleshooting the platform.

### Administrator Tasks Overview

As a CineScope administrator, you will be responsible for:

1. Setting up and maintaining the production environment
2. Monitoring system performance and health
3. Managing security and access control
4. Performing regular backups and recovery operations
5. Applying updates and patches
6. Troubleshooting issues and supporting users

### System Architecture Overview

CineScope follows a multi-tier architecture:

- **Frontend**: Blazor WebAssembly client application
- **Backend**: .NET 8.0 API hosted on Azure App Service
- **Database**: MongoDB Atlas for data storage
- **Security**: JWT-based authentication and role-based access control
- **Caching**: Multi-level caching system for performance optimization

### System Requirements

Minimum server specifications for production:
```
Hardware:
- CPU: 4 cores
- Memory: 16GB RAM
- Storage: 256GB SSD

Software:
- Runtime: .NET 8.0+
- Database: MongoDB 5.0+
- Web Server: IIS 10.0+ or Kestrel
- SSL: TLS 1.2+
```

## 2. Initial Setup

### 2A. Development Environment

**Step 1: Install Required Tools**

1. Log in to your development machine with administrative privileges
2. Open PowerShell as administrator and run the following commands:

```powershell
# Install .NET SDK
winget install Microsoft.DotNet.SDK.8

# Install Azure CLI
winget install Microsoft.AzureCLI

# Install MongoDB Tools
winget install MongoDB.Shell
```

**Step 2: Verify Installations**

Run the following commands to verify the tools were installed correctly:

```powershell
dotnet --version    # Should return 8.0.x or higher
az --version        # Check Azure CLI is installed
mongosh --version   # Check MongoDB tools are installed
```

**Step 3: Clone the Repository**

```powershell
# Navigate to your projects directory
cd C:\Projects

# Clone the repository
git clone https://github.com/your-org/cinescope.git

# Navigate to the project directory
cd cinescope
```

**Step 4: Configure Development Settings**

1. Create a local `appsettings.Development.json` file
2. Add the following configuration, replacing placeholders with actual values:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://devuser:password@localhost:27017",
    "DatabaseName": "CineScopeDb",
    "UsersCollectionName": "users",
    "MoviesCollectionName": "movies",
    "ReviewsCollectionName": "reviews",
    "BannedWordsCollectionName": "bannedWords"
  },
  "JwtSettings": {
    "Secret": "your-development-secret-key-at-least-32-characters",
    "Issuer": "cinescope-dev",
    "Audience": "cinescope-client",
    "ExpiryMinutes": 60
  }
}
```

### 2B. Azure Deployment

**Step 1: Prepare Azure Resources**

1. Log in to Azure Portal at https://portal.azure.com
2. Navigate to Resource Groups and create a new resource group:
   - Name: `CineScope-Prod`
   - Region: `West US 2` (or your preferred region)

**Step 2: Deploy Using Azure CLI**

1. Open PowerShell with administrator privileges
2. Execute the following commands:

```powershell
# Login to Azure
az login

# Set the subscription
az account set --subscription "CineScope-Prod"

# Create app service plan
az appservice plan create --name cinescope-plan --resource-group CineScope-Prod --sku S1

# Create web app
az webapp create --name cinescope-webapp --resource-group CineScope-Prod --plan cinescope-plan --runtime "DOTNET|8.0"
```

**Step 3: Configure Application Settings**

1. Set production configuration values for the web app:

```powershell
az webapp config appsettings set --name cinescope-webapp --resource-group CineScope-Prod --settings `
  "MongoDbSettings__ConnectionString=mongodb+srv://username:password@yourcluster.mongodb.net" `
  "MongoDbSettings__DatabaseName=CineScopeDb" `
  "JwtSettings__Secret=your-production-secret-key-at-least-32-characters" `
  "JwtSettings__Issuer=cinescope-prod" `
  "JwtSettings__Audience=cinescope-client" `
  "JwtSettings__ExpiryMinutes=60" `
  "ASPNETCORE_ENVIRONMENT=Production"
```

**Step 4: Publish the Application**

1. Build and publish the application:

```powershell
# Navigate to the project directory
cd C:\Projects\cinescope

# Build and publish the project
dotnet publish -c Release -o ./publish

# Zip the published files
Compress-Archive -Path ./publish/* -DestinationPath ./cinescope.zip

# Deploy to Azure
az webapp deployment source config-zip --resource-group CineScope-Prod --name cinescope-webapp --src ./cinescope.zip
```

### 2C. MongoDB Configuration

**Step 1: Set Up MongoDB Atlas**

1. Go to https://www.mongodb.com/cloud/atlas and create an account if you don't have one
2. Create a new project named "CineScope"
3. Create a new cluster (M10 tier recommended for production)
4. Configure network access to allow connections from your Azure app service IP addresses
5. Create a database user with read/write access

**Step 2: Configure the Database**

1. Connect to your MongoDB cluster using MongoDB Compass or the shell:

```
mongosh "mongodb+srv://your-cluster-url" --username admin
```

2. Create database and collections:

```javascript
// Create and use database
use CineScopeDb

// Create collections
db.createCollection("users")
db.createCollection("movies")
db.createCollection("reviews")
db.createCollection("bannedWords")
```

**Step 3: Create Indexes for Performance**

Run the following commands to create necessary indexes:

```javascript
// Create user indexes
db.users.createIndex({ "Username": 1 }, { unique: true })
db.users.createIndex({ "Email": 1 }, { unique: true })

// Create review indexes
db.reviews.createIndex({ "MovieId": 1 })
db.reviews.createIndex({ "UserId": 1 })

// Create movie indexes
db.movies.createIndex({ "Title": "text" })
db.movies.createIndex({ "Genres": 1 })
```

**Step 4: Set Up Admin User**

Create the initial admin user:

```javascript
db.users.insertOne({
  Username: "AdminUser",
  Email: "admin@cinescope.com",
  PasswordHash: "hashed_password_here", // Generate with BCrypt
  Roles: ["Admin", "User"],
  CreatedAt: new Date(),
  IsLocked: false,
  FailedLoginAttempts: 0,
  ProfilePictureUrl: "/profile-pictures/avatar8.svg"
})
```

### 2D. SSL and Security Setup

**Step 1: Obtain SSL Certificate**

For production, purchase a certificate from a trusted provider. For testing, generate a self-signed certificate:

```powershell
# Generate self-signed certificate
$cert = New-SelfSignedCertificate `
    -DnsName "cinescope.com", "*.cinescope.com" `
    -CertStoreLocation "cert:\LocalMachine\My" `
    -NotAfter (Get-Date).AddYears(1)

# Export the certificate
$password = ConvertTo-SecureString -String "CertPassword123!" -Force -AsPlainText
Export-PfxCertificate `
    -Cert $cert `
    -FilePath "C:\Certificates\cinescope-cert.pfx" `
    -Password $password
```

**Step 2: Upload Certificate to Azure**

```powershell
# Upload certificate to Azure
az webapp config ssl upload `
    --resource-group CineScope-Prod `
    --name cinescope-webapp `
    --certificate-file "C:\Certificates\cinescope-cert.pfx" `
    --certificate-password "CertPassword123!"
```

**Step 3: Bind Certificate to Domain**

```powershell
# Get certificate thumbprint
$thumbprint = (Get-ChildItem -Path cert:\LocalMachine\My | Where-Object {$_.Subject -like "*cinescope.com*"}).Thumbprint

# Bind certificate to site
az webapp config ssl bind `
    --resource-group CineScope-Prod `
    --name cinescope-webapp `
    --certificate-thumbprint $thumbprint `
    --ssl-type SNI
```

**Step 4: Configure Security Headers**

Create a web.config file in your project with security headers:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
        <add name="Content-Security-Policy" value="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';" />
      </customHeaders>
    </httpProtocol>
    <rewrite>
      <rules>
        <rule name="HTTP to HTTPS redirect" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="off" ignoreCase="true" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

**Step 5: Force HTTPS in Azure**

```powershell
# Enable HTTPS Only
az webapp update `
    --resource-group CineScope-Prod `
    --name cinescope-webapp `
    --https-only true
```

## 3. Monitoring and Maintenance

### 3A. Application Insights

**Step 1: Create Application Insights Resource**

1. Go to Azure Portal
2. Navigate to your resource group
3. Click "Create" and search for "Application Insights"
4. Complete the form:
   - Name: `cinescope-appinsights`
   - Resource Group: `CineScope-Prod`
   - Region: Same as your app service
   - Workspace: Create new or use existing
5. Click "Review + Create" then "Create"

**Step 2: Connect App Service to Application Insights**

1. In Azure Portal, go to your App Service
2. In the left menu, click on "Application Insights"
3. Click "Turn on Application Insights"
4. Select the Application Insights resource you created
5. Click "Apply"

**Step 3: Add Custom Telemetry**

1. Open your CineScope project
2. Edit `Program.cs` to add Application Insights:

```csharp
// Add this to the services configuration
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

3. Create a monitoring service (MonitoringService.cs) for custom tracking:

```csharp
public class MonitoringService
{
    private readonly TelemetryClient _telemetryClient;

    public MonitoringService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackMovieAccess(string movieId, string userId)
    {
        _telemetryClient.TrackEvent("MovieAccessed", new Dictionary<string, string>
        {
            ["MovieId"] = movieId,
            ["UserId"] = userId,
            ["Timestamp"] = DateTime.UtcNow.ToString("O")
        });
    }

    // Add other tracking methods as needed
}
```

**Step 4: Set Up Alerts**

1. In Azure Portal, go to Application Insights
2. Click on "Alerts" in the left menu
3. Click "Create > Alert rule"
4. Configure the following alerts:
   - Server response time > 3 seconds
   - Failed requests > 5% of total
   - Server exceptions > 10 per hour
   - CPU usage > 80%
5. Set appropriate action groups (email, SMS, etc.)

### 3B. Performance Monitoring

**Step 1: Create Custom Dashboard**

1. In Azure Portal, go to "Dashboard"
2. Click "New Dashboard"
3. Name it "CineScope Performance"
4. Add the following widgets:
   - Application Insights: Server response time
   - Application Insights: Failed requests
   - Application Insights: Server exceptions
   - App Service: CPU usage
   - App Service: Memory usage
   - MongoDB: Connection metrics (if available)

**Step 2: Configure Log Analytics Workspace**

1. In Azure Portal, go to "Log Analytics workspaces"
2. Create a new workspace or use existing one
3. Link your Application Insights and App Service to this workspace

**Step 3: Create Custom Queries**

1. Go to Log Analytics workspace
2. Create and save the following queries:

Response times query:
```kusto
// Response times by operation
requests
| where timestamp > ago(24h)
| summarize
    avgDuration = avg(duration),
    p95Duration = percentile(duration, 95),
    requestCount = count()
| by name
| order by avgDuration desc
```

Error monitoring query:
```kusto
// Error analysis by type
exceptions
| where timestamp > ago(24h)
| summarize
    exceptionCount = count()
| by type
| order by exceptionCount desc
```

**Step 4: Set Up Scheduled Export of Performance Data**

1. In Application Insights, go to "Continuous Export"
2. Configure export to Azure Storage for long-term analysis
3. Schedule weekly performance reports

### 3C. Database Health

**Step 1: Create MongoDB Monitoring User**

1. Connect to MongoDB Atlas
2. Create a dedicated monitoring user:

```javascript
db.createUser({
    user: "monitoring",
    pwd: "SecureMonitoringPassword123!",
    roles: [
        { role: "monitoring", db: "admin" },
        { role: "read", db: "CineScopeDb" }
    ]
})
```

**Step 2: Configure MongoDB Performance Advisor**

1. In MongoDB Atlas, go to your cluster
2. Click on "Performance Advisor"
3. Review and apply index recommendations

**Step 3: Set Up Slow Query Logging**

1. In MongoDB Atlas, enable profiling:

```javascript
db.setProfilingLevel(1, { slowms: 100 })
```

2. Configure a process to regularly check for slow queries:

```javascript
// Check for slow queries from the last day
db.system.profile.find(
    { millis: { $gt: 100 }, ts: { $gt: new Date(ISODate().getTime() - 24*60*60*1000) } }
).sort({ millis: -1 })
```

**Step 4: Implement Regular Health Checks**

Create a PowerShell script to run daily MongoDB health checks:

```powershell
# MongoDB Health Check Script
param(
    [string]$connectionString,
    [string]$username,
    [string]$password
)

# Output file for results
$outputFile = "C:\Monitoring\mongodb_health_$(Get-Date -Format 'yyyyMMdd').txt"

# Write header to output file
"MongoDB Health Check - $(Get-Date)" | Out-File -FilePath $outputFile

# Run serverStatus command
mongosh "$connectionString" --username $username --password $password --eval "db.serverStatus()" | Out-File -FilePath $outputFile -Append

# Run database stats
mongosh "$connectionString" --username $username --password $password --eval "db.stats()" | Out-File -FilePath $outputFile -Append

# Run collection stats for key collections
mongosh "$connectionString" --username $username --password $password --eval "db.users.stats()" | Out-File -FilePath $outputFile -Append
mongosh "$connectionString" --username $username --password $password --eval "db.movies.stats()" | Out-File -FilePath $outputFile -Append
mongosh "$connectionString" --username $username --password $password --eval "db.reviews.stats()" | Out-File -FilePath $outputFile -Append

Write-Host "Health check completed. Results saved to $outputFile"
```

### 3D. Log Analysis

**Step 1: Configure Centralized Logging**

1. Update `Program.cs` to enhance logging:

```csharp
// Add this to the builder configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddApplicationInsights();
builder.Logging.AddAzureWebAppDiagnostics();
```

**Step 2: Set Up Log Categories**

Define log categories in your application:

```csharp
public static class LogCategories
{
    public const string Security = "CineScope.Security";
    public const string Performance = "CineScope.Performance";
    public const string DataAccess = "CineScope.DataAccess";
    public const string UserActivity = "CineScope.UserActivity";
    public const string SystemHealth = "CineScope.SystemHealth";
}
```

**Step 3: Create Log Monitoring Workspace**

1. In Azure Portal, go to your Log Analytics workspace
2. Configure data collection rules to collect logs from:
   - Application Insights
   - App Service logs
   - MongoDB logs (if available)

**Step 4: Set Up Regular Log Reviews**

Schedule tasks for regular log review:

1. Daily review of error logs
   - Check for authentication failures
   - Review unexpected exceptions
   - Monitor API failures

2. Weekly review of performance logs
   - Analyze slow operations
   - Identify bottlenecks
   - Check for memory/CPU issues

## 4. Backup and Recovery

### 4A. Automated Backups

**Step 1: Configure MongoDB Atlas Backups**

1. Log in to MongoDB Atlas
2. Go to your cluster and click on "Backup"
3. Configure the backup policy:
   - Enable continuous backups for point-in-time recovery
   - Schedule daily snapshots at 2 AM local time
   - Set 7-day retention period for daily backups
   - Enable weekly backups with 4-week retention
   - Enable monthly backups with 12-month retention

**Step 2: Set Up Azure Backup for App Service**

1. In Azure Portal, go to your App Service
2. Click on "Backups" in the left menu
3. Click "Configure" and set:
   - Scheduled backup: Enabled
   - Backup schedule: Daily at 3 AM
   - Retention period: 30 days
   - Storage account: Create or select existing
   - Backup database: None (handled by MongoDB Atlas)

**Step 3: Configure Backup Monitoring**

1. Create a PowerShell script to verify backups:

```powershell
# Backup Verification Script
param(
    [string]$resourceGroup = "CineScope-Prod",
    [string]$webAppName = "cinescope-webapp"
)

# Get backup status
$backupStatus = az webapp backup list --resource-group $resourceGroup --webapp-name $webAppName | ConvertFrom-Json

# Check latest backup
$latestBackup = $backupStatus | Sort-Object -Property finishedTimeStamp -Descending | Select-Object -First 1

if ($latestBackup -eq $null) {
    Write-Host "ERROR: No backups found!" -ForegroundColor Red
    exit 1
}

$backupAge = (Get-Date) - [DateTime]$latestBackup.finishedTimeStamp
if ($backupAge.TotalHours -gt 30) {
    Write-Host "WARNING: Latest backup is over 30 hours old!" -ForegroundColor Yellow
} else {
    Write-Host "SUCCESS: Latest backup was completed $($backupAge.TotalHours) hours ago" -ForegroundColor Green
}

# Output backup details
Write-Host "Backup ID: $($latestBackup.id)"
Write-Host "Status: $($latestBackup.status)"
Write-Host "Size: $($latestBackup.sizeInBytes) bytes"
```

2. Schedule this script to run daily and alert on failures

**Step 4: Create Backup Documentation**

Create a document with:
- Backup schedule details
- Retention policies
- Storage locations
- Verification procedures
- Emergency contact information

### 4B. Recovery Procedures

**Step 1: Define Recovery Point Objective (RPO) and Recovery Time Objective (RTO)**

Document your recovery objectives:
- RPO: Maximum acceptable data loss (e.g., 1 hour)
- RTO: Maximum acceptable downtime (e.g., 4 hours)

**Step 2: Document MongoDB Restoration Procedure**

1. Create a step-by-step guide for MongoDB restoration:

```
MongoDB Restoration Procedure:

1. Log in to MongoDB Atlas
2. Go to "Backup" > "Restore"
3. Select the backup point:
   a. For point-in-time recovery: Select date and time
   b. For snapshot: Select the specific snapshot
4. Choose restore type:
   a. Restore to same cluster (for recovery after data corruption)
   b. Restore to new cluster (for parallel testing before switchover)
5. Verify restored data for integrity
6. If restoring to new cluster, update connection strings in application
```

**Step 3: Document App Service Recovery Procedure**

Create an App Service recovery guide:

```
App Service Recovery Procedure:

1. Log in to Azure Portal
2. Go to App Service > Backups
3. Select the backup to restore
4. Click "Restore"
5. Choose restore options:
   a. Restore to this app (overwrite)
   b. Restore to another app (for testing)
6. Confirm the operation
7. Verify application functionality after restore
```

**Step 4: Create Recovery Testing Schedule**

Schedule regular recovery drills:
- Quarterly MongoDB restore test
- Quarterly App Service restore test
- Document results and improvement areas after each test

### 4C. Disaster Recovery

**Step 1: Define Disaster Scenarios**

Document potential disaster scenarios:
1. Primary region outage
2. Database corruption
3. Security breach
4. Application code failure
5. Infrastructure failure

**Step 2: Create Secondary Region Infrastructure**

1. Create a duplicate environment in a secondary region:

```powershell
# Set variables
$primaryRegion = "westus2"
$secondaryRegion = "eastus2"
$resourceGroup = "CineScope-DR"

# Create resource group in secondary region
az group create --name $resourceGroup --location $secondaryRegion

# Create app service plan
az appservice plan create --name cinescope-plan-dr --resource-group $resourceGroup --sku S1 --location $secondaryRegion

# Create web app
az webapp create --name cinescope-webapp-dr --resource-group $resourceGroup --plan cinescope-plan-dr --runtime "DOTNET|8.0"

# Configure app settings (similar to production)
az webapp config appsettings set --name cinescope-webapp-dr --resource-group $resourceGroup --settings `
  "MongoDbSettings__ConnectionString=mongodb+srv://username:password@yourcluster.mongodb.net" `
  "MongoDbSettings__DatabaseName=CineScopeDb" `
  "JwtSettings__Secret=your-dr-secret-key-at-least-32-characters" `
  "JwtSettings__Issuer=cinescope-dr" `
  "JwtSettings__Audience=cinescope-client" `
  "ASPNETCORE_ENVIRONMENT=Production"
```

**Step 3: Configure Traffic Manager**

1. Create a Traffic Manager profile:

```powershell
# Create Traffic Manager profile
az network traffic-manager profile create `
    --name cinescope-tm `
    --resource-group CineScope-Prod `
    --routing-method Priority `
    --unique-dns-name cinescope

# Add primary endpoint
az network traffic-manager endpoint create `
    --name primary `
    --profile-name cinescope-tm `
    --resource-group CineScope-Prod `
    --type azureEndpoints `
    --target-resource-id "/subscriptions/{subscription-id}/resourceGroups/CineScope-Prod/providers/Microsoft.Web/sites/cinescope-webapp" `
    --priority 1

# Add secondary endpoint
az network traffic-manager endpoint create `
    --name secondary `
    --profile-name cinescope-tm `
    --resource-group CineScope-Prod `
    --type azureEndpoints `
    --target-resource-id "/subscriptions/{subscription-id}/resourceGroups/CineScope-DR/providers/Microsoft.Web/sites/cinescope-webapp-dr" `
    --priority 2
```

**Step 4: Create Disaster Recovery Playbooks**

Create detailed procedure documents for each disaster scenario:

Example for Primary Region Outage:
```
Primary Region Outage Recovery Procedure:

1. Verify outage is not a false alarm
2. Initiate Incident Response protocol:
   a. Notify stakeholders
   b. Create incident ticket
   c. Assign recovery team
3. Execute failover:
   a. Manually trigger Traffic Manager failover to secondary region
   b. Verify DNS propagation
   c. Confirm application accessibility
4. Monitor secondary region performance
5. When primary region is restored:
   a. Sync any data changes back to primary region
   b. Test primary region functionality
   c. Execute planned failback during low-traffic window
6. Document incident details and lessons learned
```

**Step 5: Schedule DR Drills**

Schedule regular disaster recovery drills:
- Semi-annual failover testing
- Annual full disaster recovery simulation
- Document lessons learned and improve processes

## 5. Security Management

### 5A. Access Control

**Step 1: Implement Role-Based Access Control (RBAC)**

1. Define user roles in the application:

```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("ContentModerator", policy => 
        policy.RequireRole("Moderator", "Admin"));
    
    options.AddPolicy("ReviewManager", policy => 
        policy.RequireRole("ReviewManager", "Admin"));
});
```

**Step 2: Configure JWT Authentication**

1. Set up JWT authentication in the application:

```csharp
// In Program.cs
// Register authentication services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };
    });
```

**Step 3: Implement Admin User Management**

1. Create a procedure for adding new admin users:

```
Admin User Creation Procedure:

1. Connect to MongoDB using a secure connection
2. Generate a secure password for the new admin
3. Hash the password using BCrypt:
   - In C# code: BCrypt.Net.BCrypt.HashPassword(password)
   - Or use MongoDB shell with a hashing function
4. Insert the new admin user document:
   db.users.insertOne({
     Username: "newadmin",
     Email: "newadmin@cinescope.com",
     PasswordHash: "<generated_hash>",
     Roles: ["Admin", "User"],
     CreatedAt: new Date(),
     IsLocked: false,
     FailedLoginAttempts: 0
   })
5. Verify the user can log in
6. Document the new admin user in the secure records
```

**Step 4: Create Access Control Audit Process**

1. Implement regular auditing of user roles and permissions:
   - Monthly review of admin users
   - Quarterly audit of role assignments
   - Verification that no unnecessary privileges exist

2. Document the audit process:

```
Access Control Audit Procedure:

1. Generate list of all admin users:
   db.users.find({ Roles: "Admin" })

2. Verify each admin user is still active and required:
   - Check last login date
   - Confirm employment status
   - Validate need for admin access

3. Review role definitions for least privilege
   - Ensure roles contain minimum required permissions
   - Remove any excessive permissions

4. Document audit results and actions taken
```

### 5B. Threat Monitoring

**Step 1: Configure Login Attempt Monitoring**

1. Implement login attempt tracking in the authentication service:

```csharp
public async Task<AuthResponse> LoginAsync(Login

### 8A. Search Configuration (continued)

**Step 2: Implement Search Service (continued)**

```csharp
        // Create result
        var result = new SearchResult<MovieDto>
        {
            Items = movieDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        // Cache the result
        _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    private FilterDefinition<Movie> BuildSearchFilter(SearchRequest request)
    {
        var builder = Builders<Movie>.Filter;
        var filters = new List<FilterDefinition<Movie>>();

        // Text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            filters.Add(builder.Text(request.Query));
        }

        // Genre filter
        if (request.Genres != null && request.Genres.Any())
        {
            filters.Add(builder.AnyIn(m => m.Genres, request.Genres));
        }

        // Year range filter
        if (request.YearFrom.HasValue)
        {
            var fromDate = new DateTime(request.YearFrom.Value, 1, 1);
            filters.Add(builder.Gte(m => m.ReleaseDate, fromDate));
        }

        if (request.YearTo.HasValue)
        {
            var toDate = new DateTime(request.YearTo.Value, 12, 31);
            filters.Add(builder.Lte(m => m.ReleaseDate, toDate));
        }

        // Rating filter
        if (request.MinRating.HasValue)
        {
            filters.Add(builder.Gte(m => m.AverageRating, request.MinRating.Value));
        }

        // Combine all filters
        return filters.Any() 
            ? builder.And(filters) 
            : builder.Empty;
    }

    private SortDefinition<Movie> GetSortDefinition(string sortBy)
    {
        return sortBy switch
        {
            "title_asc" => Builders<Movie>.Sort.Ascending(m => m.Title),
            "title_desc" => Builders<Movie>.Sort.Descending(m => m.Title),
            "rating_desc" => Builders<Movie>.Sort.Descending(m => m.AverageRating),
            "rating_asc" => Builders<Movie>.Sort.Ascending(m => m.AverageRating),
            "year_desc" => Builders<Movie>.Sort.Descending(m => m.ReleaseDate),
            "year_asc" => Builders<Movie>.Sort.Ascending(m => m.ReleaseDate),
            _ => Builders<Movie>.Sort.Descending(m => m.AverageRating)
        };
    }
}
```

**Step 3: Configure Search API Endpoints**

1. Create search controller:

```csharp
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(SearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet("movies")]
    public async Task<ActionResult<SearchResult<MovieDto>>> SearchMovies(
        [FromQuery] string query = "",
        [FromQuery] List<string> genres = null,
        [FromQuery] int? yearFrom = null,
        [FromQuery] int? yearTo = null,
        [FromQuery] double? minRating = null,
        [FromQuery] string sortBy = "rating_desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation(
                "Search request: Query={Query}, Genres={Genres}, YearRange={YearFrom}-{YearTo}, " +
                "MinRating={MinRating}, SortBy={SortBy}, Page={Page}, PageSize={PageSize}",
                query, string.Join(",", genres ?? new List<string>()), yearFrom, yearTo, 
                minRating, sortBy, page, pageSize);

            // Create search request
            var request = new SearchRequest
            {
                Query = query,
                Genres = genres,
                YearFrom = yearFrom,
                YearTo = yearTo,
                MinRating = minRating,
                SortBy = sortBy,
                Page = Math.Max(1, page),
                PageSize = Math.Clamp(pageSize, 1, 100)
            };

            // Perform search
            var result = await _searchService.SearchMoviesAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies");
            return StatusCode(500, new { Error = "Error searching movies" });
        }
    }
}
```

**Step 4: Create Search Testing Procedure**

Document the search testing process:

```
Search Testing Procedure:

1. Basic Query Testing
   - Simple keyword search (e.g., "action", "star wars")
   - Multi-word search (e.g., "science fiction")
   - Partial matches (e.g., "trek" for "Star Trek")
   - Case insensitivity (e.g., "MATRIX" vs "matrix")

2. Filter Testing
   - Genre filtering (single and multiple genres)
   - Year range filtering
   - Rating filtering
   - Combined filters (e.g., "action movies after 2010 with rating > 4")

3. Sorting Testing
   - Sort by title (ascending and descending)
   - Sort by rating (ascending and descending)
   - Sort by release year (ascending and descending)

4. Pagination Testing
   - Navigate through multiple pages
   - Change page size
   - Verify correct item counts

5. Performance Testing
   - Measure response times for different query types
   - Test with large result sets
   - Verify cache behavior
```

### 8B. Filter Management

**Step 1: Implement Genre Management**

1. Create genre management service:

```csharp
public class GenreService
{
    private readonly IMongoDbService _mongoDbService;
    private readonly MongoDbSettings _settings;
    private readonly ICacheService _cacheService;

    private const string GENRES_CACHE_KEY = "AllGenres";

    public async Task<List<string>> GetAllGenresAsync()
    {
        // Try to get from cache first
        if (_cacheService.TryGetValue(GENRES_CACHE_KEY, out List<string> cachedGenres))
        {
            return cachedGenres;
        }

        // Get movies collection
        var collection = _mongoDbService.GetCollection<Movie>(_settings.MoviesCollectionName);

        // Get distinct genres
        var genres = await collection.Distinct<string>("Genres", FilterDefinition<Movie>.Empty)
            .ToListAsync();

        // Sort alphabetically
        genres.Sort();

        // Cache for 24 hours
        _cacheService.Set(GENRES_CACHE_KEY, genres, TimeSpan.FromHours(24));

        return genres;
    }

    public async Task<bool> AddGenreAsync(string genre)
    {
        // Validate genre
        if (string.IsNullOrWhiteSpace(genre) || genre.Length > 50)
        {
            return false;
        }

        // Get all existing genres
        var genres = await GetAllGenresAsync();

        // Check if genre already exists
        if (genres.Contains(genre, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        // Add to system configuration
        // NOTE: In a real system, we'd store genres as a separate collection
        // This is a simplified example

        // Invalidate cache
        _cacheService.Remove(GENRES_CACHE_KEY);

        return true;
    }
}
```

**Step 2: Create Filter API**

1. Implement filter controller:

```csharp
[ApiController]
[Route("api/filters")]
public class FilterController : ControllerBase
{
    private readonly GenreService _genreService;
    private readonly ILogger<FilterController> _logger;

    [HttpGet("genres")]
    public async Task<ActionResult<List<string>>> GetAllGenres()
    {
        try
        {
            var genres = await _genreService.GetAllGenresAsync();
            return Ok(genres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genres");
            return StatusCode(500, new { Error = "Error retrieving genres" });
        }
    }

    [HttpPost("genres")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddGenre([FromBody] string genre)
    {
        try
        {
            var result = await _genreService.AddGenreAsync(genre);
            if (result)
            {
                return Ok(new { Message = "Genre added successfully" });
            }
            return BadRequest(new { Error = "Genre already exists or is invalid" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding genre");
            return StatusCode(500, new { Error = "Error adding genre" });
        }
    }
}
```

**Step 3: Document Filter Administration**

Create a guide for filter management:

```
Filter Management Procedures:

1. Viewing Available Filters
   - Navigate to Admin > Filter Management
   - All current genres, year ranges, and rating filters are displayed
   - Usage statistics show how often each filter is applied

2. Adding New Genres
   - In Admin > Filter Management > Genres tab
   - Click "Add Genre"
   - Enter new genre name
   - Click Save
   - New genre will be available immediately for movie classification

3. Renaming or Merging Genres
   - In Admin > Filter Management > Genres tab
   - Select the genre(s) to modify
   - For rename: Click "Rename" and enter new name
   - For merge: Select multiple genres and click "Merge"
   - Follow confirmation prompts
   - System will update all movie records automatically

4. Filter Analytics
   - In Admin > Filter Management > Analytics tab
   - View charts showing filter usage over time
   - Identify most popular filters
   - Track search patterns
```

**Step 4: Create User Preference Management**

Implement user filter preferences:

```csharp
public class UserPreferenceService
{
    private readonly IMongoDbService _mongoDbService;
    private readonly MongoDbSettings _settings;

    public async Task<UserPreferences> GetUserPreferencesAsync(string userId)
    {
        var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
        var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        // If user has no preferences yet, return defaults
        if (user?.Preferences == null)
        {
            return new UserPreferences
            {
                FavoriteGenres = new List<string>(),
                PreferredSortOrder = "rating_desc",
                PreferredPageSize = 20,
                ExcludedGenres = new List<string>()
            };
        }

        return user.Preferences;
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
    {
        var collection = _mongoDbService.GetCollection<User>(_settings.UsersCollectionName);
        
        var update = Builders<User>.Update
            .Set(u => u.Preferences, preferences);

        var result = await collection.UpdateOneAsync(u => u.Id == userId, update);
        
        return result.ModifiedCount > 0;
    }
}
```

### 8C. Performance Tuning

**Step 1: Create Index Optimization Procedure**

Document the index optimization process:

```
MongoDB Index Optimization Procedure:

1. Review Current Indexes
   - Connect to MongoDB Atlas
   - Run db.movies.getIndexes() to list all indexes
   - Evaluate current index size and coverage

2. Analyze Index Usage
   - Run db.movies.aggregate([{$indexStats:{}}]) to get usage statistics
   - Identify unused or rarely used indexes
   - Identify slow queries from logs

3. Optimize Indexes
   - Remove unused indexes: db.movies.dropIndex("index_name")
   - Create compound indexes for common query patterns
   - Use text indexes for search functionality
   - Consider partial indexes for better selectivity

4. Test Performance
   - Before making changes, benchmark current performance
   - After making changes, compare query performance
   - Document improvements or regressions
```

**Step 2: Configure Query Monitoring**

Implement query monitoring:

```csharp
public class QueryPerformanceMonitor
{
    private readonly ILogger<QueryPerformanceMonitor> _logger;
    private readonly TelemetryClient _telemetryClient;

    public async Task<T> TrackQueryPerformance<T>(string queryName, Func<Task<T>> queryFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Execute the query
            var result = await queryFunc();
            
            // Stop timing
            stopwatch.Stop();
            
            // Log performance metrics
            _logger.LogInformation(
                "Query {QueryName} executed in {ElapsedMilliseconds}ms",
                queryName, stopwatch.ElapsedMilliseconds);
            
            // Track in Application Insights
            _telemetryClient.TrackDependency(
                "MongoDB", 
                "Query", 
                queryName, 
                DateTimeOffset.UtcNow,
                stopwatch.Elapsed, 
                true);
            
            return result;
        }
        catch (Exception ex)
        {
            // Stop timing
            stopwatch.Stop();
            
            // Log failure
            _logger.LogError(ex, 
                "Query {QueryName} failed after {ElapsedMilliseconds}ms",
                queryName, stopwatch.ElapsedMilliseconds);
            
            // Track failure in Application Insights
            _telemetryClient.TrackDependency(
                "MongoDB", 
                "Query", 
                queryName, 
                DateTimeOffset.UtcNow,
                stopwatch.Elapsed, 
                false);
            
            throw;
        }
    }
}
```

**Step 3: Implement Resource Monitoring**

Create a resource monitoring script:

```powershell
# Azure App Service Performance Monitoring Script
param(
    [string]$resourceGroup = "CineScope-Prod",
    [string]$webAppName = "cinescope-webapp",
    [string]$outputPath = "C:\Monitoring\performance"
)

# Ensure output directory exists
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath | Out-Null
}

# Set time range
$startTime = (Get-Date).AddHours(-24).ToString("yyyy-MM-ddTHH:mm:ssZ")
$endTime = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
$timefilter = "(timestamp ge $startTime and timestamp le $endTime)"

# Get CPU metrics
$cpuMetrics = az monitor metrics list `
    --resource $webAppName `
    --resource-group $resourceGroup `
    --resource-type "Microsoft.Web/sites" `
    --metric "CpuPercentage" `
    --interval PT5M `
    --aggregation Average `
    --start-time $startTime `
    --end-time $endTime | ConvertFrom-Json

# Get memory metrics
$memoryMetrics = az monitor metrics list `
    --resource $webAppName `
    --resource-group $resourceGroup `
    --resource-type "Microsoft.Web/sites" `
    --metric "MemoryPercentage" `
    --interval PT5M `
    --aggregation Average `
    --start-time $startTime `
    --end-time $endTime | ConvertFrom-Json

# Get request metrics
$requestMetrics = az monitor metrics list `
    --resource $webAppName `
    --resource-group $resourceGroup `
    --resource-type "Microsoft.Web/sites" `
    --metric "HttpResponseTime" `
    --interval PT5M `
    --aggregation Average `
    --start-time $startTime `
    --end-time $endTime | ConvertFrom-Json

# Output to CSV
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$cpuFile = Join-Path $outputPath "cpu_$timestamp.csv"
$memoryFile = Join-Path $outputPath "memory_$timestamp.csv"
$requestFile = Join-Path $outputPath "requests_$timestamp.csv"

# Write CSV files
$cpuMetrics.value.timeseries.data | 
    Select-Object @{Name="Timestamp";Expression={$_.timeStamp}}, @{Name="CPU";Expression={$_.average}} | 
    Export-Csv -Path $cpuFile -NoTypeInformation

$memoryMetrics.value.timeseries.data | 
    Select-Object @{Name="Timestamp";Expression={$_.timeStamp}}, @{Name="Memory";Expression={$_.average}} | 
    Export-Csv -Path $memoryFile -NoTypeInformation

$requestMetrics.value.timeseries.data | 
    Select-Object @{Name="Timestamp";Expression={$_.timeStamp}}, @{Name="ResponseTime";Expression={$_.average}} | 
    Export-Csv -Path $requestFile -NoTypeInformation

Write-Host "Performance metrics exported to $outputPath"
```

**Step 4: Create Performance Tuning Guide**

Document performance tuning steps:

```
Performance Tuning Procedure:

1. Identify Performance Bottlenecks
   - Review Application Insights performance data
   - Check MongoDB query performance
   - Analyze Azure App Service metrics
   - Focus on slowest operations first

2. Database Performance Tuning
   - Optimize indexes based on query patterns
   - Review and optimize slow queries
   - Consider data model changes if needed
   - Implement caching for frequently accessed data

3. Application Performance Tuning
   - Profile code to identify slow methods
   - Review and optimize LINQ queries
   - Implement asynchronous operations where appropriate
   - Consider parallel processing for batch operations

4. Infrastructure Tuning
   - Scale up/out Azure resources if needed
   - Optimize MongoDB Atlas tier/configuration
   - Configure auto-scaling rules
   - Implement CDN for static assets

5. Measure and Document Improvements
   - Run performance tests before and after changes
   - Document performance improvements
   - Monitor for regression
```

## 9. System Updates

### 9A. Deployment Process

**Step 1: Create Deployment Pipeline**

1. Set up Azure DevOps pipeline or GitHub Actions:

Azure DevOps YAML:
```yaml
trigger:
  branches:
    include:
    - main
    - release/*

variables:
  solution: 'CineScope.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'
  azureSubscription: 'CineScope-Production'
  appServiceName: 'cinescope-webapp'

stages:
- stage: Build
  jobs:
  - job: Build
    pool:
      vmImage: 'windows-latest'
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: 'sdk'
        version: '8.0.x'
        
    - task: DotNetCoreCLI@2
      displayName: 'Restore Packages'
      inputs:
        command: 'restore'
        projects: '$(solution)'
        
    - task: DotNetCoreCLI@2
      displayName: 'Build'
      inputs:
        command: 'build'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration) --no-restore'
        
    - task: DotNetCoreCLI@2
      displayName: 'Test'
      inputs:
        command: 'test'
        projects: '**/*Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build'
        
    - task: DotNetCoreCLI@2
      displayName: 'Publish'
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true
        
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Artifacts'
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'drop'

- stage: Deploy_Staging
  dependsOn: Build
  jobs:
  - deployment: DeployStaging
    environment: 'Staging'
    pool:
      vmImage: 'windows-latest'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureRmWebAppDeployment@4
            inputs:
              ConnectionType: 'AzureRM'
              azureSubscription: '$(azureSubscription)'
              appType: 'webApp'
              WebAppName: '$(appServiceName)-staging'
              packageForLinux: '$(Pipeline.Workspace)/drop/*.zip'
              deploymentMethod: 'auto'

- stage: Test
  dependsOn: Deploy_Staging
  jobs:
  - job: RunTests
    pool:
      vmImage: 'windows-latest'
    steps:
    - task: PowerShell@2
      displayName: 'Run Integration Tests'
      inputs:
        targetType: 'inline'
        script: |
          # Run integration tests against staging environment
          $stagingUrl = "https://$(appServiceName)-staging.azurewebsites.net"
          Write-Host "Running tests against $stagingUrl"
          # Add test script here

- stage: Deploy_Production
  dependsOn: Test
  jobs:
  - deployment: DeployProduction
    environment: 'Production'
    pool:
      vmImage: 'windows-latest'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureRmWebAppDeployment@4
            inputs:
              ConnectionType: 'AzureRM'
              azureSubscription: '$(azureSubscription)'
              appType: 'webApp'
              WebAppName: '$(appServiceName)'
              packageForLinux: '$(Pipeline.Workspace)/drop/*.zip'
              deploymentMethod: 'auto'
```

**Step 2: Document Release Process**

Create a release management guide:

```
Release Management Process:

1. Release Planning
   - Determine release scope and version number
   - Create release branch (release/vX.Y.Z)
   - Freeze features for the release
   - Notify stakeholders of upcoming release

2. Pre-Release Testing
   - Run full test suite in development/test environment
   - Perform security scan
   - Conduct performance testing
   - Fix critical issues found during testing

3. Deployment Process
   - Schedule release window (typically 2AM-4AM)
   - Notify users of planned maintenance
   - Execute deployment pipeline
   - Deploy to staging environment first
   - Verify staging deployment
   - Deploy to production environment
   - Verify production deployment

4. Post-Deployment Verification
   - Run smoke tests
   - Monitor application health
   - Check for any errors in logs
   - Verify critical features

5. Release Documentation
   - Update release notes
   - Document known issues
   - Notify users of completed deployment
   - Update system documentation
```

**Step 3: Configure Deployment Notification**

Create a deployment notification script:

```powershell
# Deployment Notification Script
param(
    [string]$environment = "Production",
    [string]$version = "1.0.0",
    [string]$releaseNotes = "",
    [string]$emailRecipients = "it-team@cinescope.com,operations@cinescope.com"
)

# Set email parameters
$subject = "CineScope $version Deployed to $environment"
$from = "deployments@cinescope.com"
$to = $emailRecipients.Split(',')

# Create email body
$body = @"
<html>
<body>
<h2>CineScope $version has been deployed to $environment</h2>
<p><strong>Deployment Time:</strong> $(Get-Date)</p>
<p><strong>Environment:</strong> $environment</p>
<p><strong>Version:</strong> $version</p>

<h3>Release Notes:</h3>
<pre>$releaseNotes</pre>

<p>Please verify the system is functioning correctly and report any issues to IT Support.</p>
</body>
</html>
"@

# Send email
$smtpServer = "smtp.cinescope.com"
$smtpPort = 587
$smtpUsername = "deployments@cinescope.com"
$smtpPassword = ConvertTo-SecureString $env:SMTP_PASSWORD -AsPlainText -Force
$credentials = New-Object System.Management.Automation.PSCredential($smtpUsername, $smtpPassword)

Send-MailMessage -From $from -To $to -Subject $subject -Body $body -BodyAsHtml -SmtpServer $smtpServer -Port $smtpPort -Credential $credentials -UseSsl

Write-Host "Deployment notification sent"
```

**Step 4: Set Up Configuration Management**

Document configuration management process:

```
Configuration Management Procedure:

1. Environment-Specific Configurations
   - Development: appsettings.Development.json
   - Staging: appsettings.Staging.json
   - Production: appsettings.Production.json

2. Sensitive Configuration Management
   - Store sensitive values in Azure Key Vault
   - Reference Key Vault secrets in application
   - Never commit secrets to source control
   - Configure identity for accessing Key Vault

3. Configuration Updates Process
   - For non-sensitive changes:
     * Update configuration files in source control
     * Deploy through standard pipeline
   - For sensitive changes:
     * Update secrets in Azure Key Vault
     * No redeployment needed (secrets loaded at runtime)

4. Configuration Versioning
   - Track configuration changes in version control
   - Document significant configuration changes
   - Include configuration version in system info
```

### 9B. Testing Protocol

**Step 1: Define Testing Phases**

Document the testing protocol:

```
CineScope Testing Protocol:

1. Unit Testing
   - Automated tests for individual components
   - Run in CI/CD pipeline on every commit
   - Minimum 75% code coverage required
   - Must pass before deployment

2. Integration Testing
   - Automated tests for component interaction
   - Run in CI/CD pipeline before deployment
   - Tests API endpoints and database operations
   - Uses test database with sample data

3. UI Testing
   - Automated tests for critical user workflows
   - Uses Playwright or similar framework
   - Tests basic user journeys:
     * User registration and login
     * Movie browsing and searching
     * Review creation and editing

4. Performance Testing
   - Load testing with simulated users
   - Measures response times under load
   - Tests database performance
   - Run before major releases
   - Baseline: 500 concurrent users with <3s response time

5. Security Testing
   - Automated vulnerability scanning
   - OWASP Top 10 checks
   - JWT implementation testing
   - Run before production deployment
```

**Step 2: Create Testing Scripts**

1. PowerShell script for post-deployment testing:

```powershell
# Post-Deployment Testing Script
param(
    [string]$baseUrl = "https://cinescope-webapp.azurewebsites.net",
    [string]$testUsername = "testuser",
    [string]$testPassword = "TestPassword123!"
)

# Function to test endpoint
function Test-Endpoint {
    param(
        [string]$endpoint,
        [string]$method = "GET",
        [hashtable]$headers = @{},
        [object]$body = $null,
        [int]$expectedStatusCode = 200
    )

    $url = "$baseUrl/$endpoint"
    
    try {
        $params = @{
            Uri = $url
            Method = $method
            Headers = $headers
            UseBasicParsing = $true
        }
        
        if ($body -and $method -ne "GET") {
            $jsonBody = $body | ConvertTo-Json -Depth 10
            $params.Add("Body", $jsonBody)
            $params.Add("ContentType", "application/json")
        }
        
        $response = Invoke-WebRequest @params
        
        if ($response.StatusCode -eq $expectedStatusCode) {
            Write-Host "✓ $method $url - Status: $($response.StatusCode)" -ForegroundColor Green
            return $response
        } else {
            Write-Host "✗ $method $url - Expected: $expectedStatusCode, Got: $($response.StatusCode)" -ForegroundColor Red
            return $null
        }
    } catch {
        Write-Host "✗ $method $url - Error: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Test public endpoints
$healthCheck = Test-Endpoint "api/health"
$genresCheck = Test-Endpoint "api/filters/genres"
$moviesCheck = Test-Endpoint "api/movie"

# Test authentication
$loginBody = @{
    username = $testUsername
    password = $testPassword
}

$loginResponse = Test-Endpoint "api/auth/login" -method "POST" -body $loginBody
if ($loginResponse) {
    $authToken = ($loginResponse.Content | ConvertFrom-Json).token
    
    if ($authToken) {
        $authHeaders = @{
            "Authorization" = "Bearer $authToken"
        }
        
        # Test authenticated endpoints
        $profileCheck = Test-Endpoint "api/user/profile" -headers $authHeaders
        $reviewsCheck = Test-Endpoint "api/review/user/current" -headers $authHeaders
    }
}

# Output test summary
Write-Host "`nTest Summary:"
Write-Host "Health Check: $($healthCheck -ne $null ? 'Passed' : 'Failed')"
Write-Host "Genres API: $($genresCheck -ne $null ? 'Passed' : 'Failed')"
Write-Host "Movies API: $($moviesCheck -ne $null ? 'Passed' : 'Failed')"
Write-Host "Login: $($loginResponse -ne $null ? 'Passed' : 'Failed')"
Write-Host "Profile API: $($profileCheck -ne $null ? 'Passed' : 'Failed')"
Write-Host "Reviews API: $($reviewsCheck -ne $null ? 'Passed' : 'Failed')"
```

**Step 3: Configure Automated UI Testing**

1. Create Playwright testing script:

```javascript
// Playwright UI testing script
const { test, expect } = require('@playwright/test');

test.describe('CineScope Basic User Journey', () => {
  let page;

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();
    await page.goto('https://cinescope-webapp.azurewebsites.net');
  });

  test('Homepage loads correctly', async () => {
    // Check title
    await expect(page).toHaveTitle(/CineScope/);
    
    // Check for main sections
    await expect(page.locator('text=Top Rated Movies')).toBeVisible();
    await expect(page.locator('text=Recently Added')).toBeVisible();
  });

  test('Search functionality works', async () => {
    // Navigate to movies page
    await page.click('text=Movies');
    
    // Enter search term
    await page.fill('input[placeholder="Search Movies.."]', 'action');
    await page.press('input[placeholder="Search Movies.."]', 'Enter');
    
    // Verify search results appear
    await expect(page.locator('.movie-card')).toHaveCount({ minimum: 1 });
  });
  
  test('User can log in', async () => {
    // Click login button
    await page.click('text=Login');

