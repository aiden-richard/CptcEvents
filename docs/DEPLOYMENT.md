# CptcEvents Cloud Architecture Documentation

## Overview

This document provides a high-level overview of the CptcEvents deployment architecture, including cloud infrastructure and deployment pipeline. CptcEvents is a full-stack web application for managing campus events and groups, built with modern  technologies.

For local development setup instructions, see the [Local Development Guide](DEVELOPMENT.md).

## Technology Stack

- **Application Framework**: ASP.NET Core 10.0 (MVC)
- **Database**: Azure SQL Database (SQL Server)
- **Authentication**: ASP.NET Core Identity
- **Email Service**: SendGrid
- **Container Platform**: Docker
- **Cloud Provider**: Microsoft Azure
- **CI/CD**: GitHub Actions
- **Domain & CDN**: Cloudflare

## System Architecture

### Architecture Diagram

```
User Browser (cptcevents.org)
        ↓
[Cloudflare Tunnel]
        ↓
[Caddy Reverse Proxy]
    ├─→ Health Check Detection
    └─→ Cold Start Handler
        ↓
[Azure Container Apps]
    ├─→ Auto-scale down when not in use
    └─→ ASP.NET Core Application
        ↓
[Azure SQL Database]
    └─→ Entity Framework Core
```

## Infrastructure Components


### Azure Resource Group

A logical container (`CptcEventsResourceGroup`) that groups all related Azure resources for simplified management, billing tracking, and access control. All production infrastructure resides within this single resource group in the `westus2` region.


### Azure SQL Database

The application uses Azure SQL Database as its primary data store. This replaced the initial SQLite implementation to support production workloads with proper scaling, backup, and high availability.

The database is managed through Entity Framework Core migrations, which are applied automatically on application startup via `context.Database.MigrateAsync()` in Program.cs. This ensures the schema is always synchronized with the application code.


### Azure Container Registry (ACR)

A private Docker registry stores and manages container images for the application. The CI/CD pipeline pushes built images to ACR, and Azure Container Apps pulls from it during deployment.

### Azure Container Apps

The application runs on Azure Container Apps, a managed platform that handles server infrastructure automatically. Container Apps will take care of configuring and maintaining servers.

**Key Features**:
- **Auto-scaling**: Automatically adds or removes running instances based on traffic (can scale down to zero when not in use)
- **Serverless Billing**: Pay only for the time the application is actually running
- **Zero-downtime Deployments**: New versions are deployed without interrupting active users

### Cost Management

The infrastructure includes budget monitoring configured at $20/month with alerts at 85% threshold (~$17). This prevents unexpected charges by providing early warnings before spending exceeds the allocated budget, essential for Azure's usage-based billing model.


## Cold Start Handling Infrastructure

One challenge with serverless container platforms is cold start latency when scaling from zero. CptcEvents implements a solution using a reverse proxy architecture to provide users with an interactive loading experience during cold starts.

**Architecture**:

```
User Browser (cptcevents.org)
        ↓
Cloudflare Tunnel
        ↓
Caddy Reverse Proxy (in Docker)
    ├─→ Azure Container App (Ready) → User sees application
    └─→ Azure Container App (Cold Start) → Interactive loading page
        └─→ Client device requests site again every 3 seconds (invisible to user)
        └─→ Automatic redirect when ready
```

**Components**:

- **Cloudflare Tunnel**: Provides secure public access to the proxy infrastructure without exposing servers directly, handling DNS routing from `cptcevents.org`
- **Caddy Reverse Proxy**: Monitors Azure Container Apps availability by detecting 503 errors, serving either the application or loading page accordingly
- **Interactive Loading Page**: HTML5/JavaScript interface with animations and automatic polling that redirects users once the application becomes available

**Implementation**: Deployed as a separate infrastructure layer using Docker Compose, with configuration managed in the [CptcEvents-Loading-Page](https://github.com/aiden-richard/CptcEvents-Loading-Page) repository.

This solution significantly improves user experience by replacing timeout errors with a polished loading interface that automatically brings users to the application once it's ready.


## CI/CD Pipeline

The application uses GitHub Actions for continuous integration and deployment, with two separate workflows handling different stages of the development lifecycle.

### Build Workflow

**Trigger**: Pull requests to `main` branch

**Purpose**: Validates code quality before merging by ensuring the application compiles successfully. This prevents broken code from entering the main branch.

**Pipeline Stages**:

1. **Checkout & Setup** - Checkout source code and install .NET 10 Preview SDK

2. **Build & Test** - Restore NuGet dependencies, build in Release configuration, and run tests

### Deployment Workflow

**Trigger**: Pushes to `main` branch (typically after PR merge)

**Purpose**: Automatically builds, containerizes, and deploys the application to production.

**Pipeline Stages**:

1. **Build** - Checkout code, install .NET 10 SDK, restore dependencies, and compile Release build

2. **Containerize** - Authenticate with Azure/ACR, build multi-stage Docker image, tag with commit SHA and `latest`, push to registry

3. **Deploy** - Update Azure Container Apps with new image, inject environment variables (database connection, API keys, admin credentials)

### Secrets Management

The pipeline uses GitHub Secrets to securely manage sensitive configuration:

- `AZURE_CREDENTIALS` - Service principal credentials for Azure authentication
- `ACR_PASSWORD` - Container registry authentication
- `SQL_CONNECTION_STRING` - Database connection details
- `SENDGRID_API_KEY` - Email service API key
- `ADMIN_PASSWORD` - Initial admin account password

These secrets are injected as environment variables into the running container at deployment time.

## Additional Resources

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [CptcEvents Loading Page Repository](https://github.com/aiden-richard/CptcEvents-Loading-Page)
connect-apps/)

---

*For local development setup and troubleshooting, see [DEVELOPMENT.md](DEVELOPMENT.md).*