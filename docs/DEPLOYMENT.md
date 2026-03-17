# CptcEvents Deployment Documentation

## Overview

This document provides a high-level overview of the CptcEvents deployment architecture, including infrastructure and the CI/CD pipeline. CptcEvents is a full-stack web application for managing campus events and groups, built with modern technologies.

For local development setup instructions, see the [Local Development Guide](DEVELOPMENT.md).

## Technology Stack

- **Application Framework**: ASP.NET Core 10.0 (MVC)
- **Database**: SQL Server 2022 (self-hosted via Docker)
- **Authentication**: ASP.NET Core Identity
- **Email Service**: SendGrid
- **Container Platform**: Docker
- **CI/CD**: GitHub Actions (self-hosted runner)
- **Domain & CDN**: Cloudflare

## System Architecture

### Architecture Diagram

```
User Browser (cptcevents.org)
        ↓
[Cloudflare Tunnel]  (cloudflared container)
        ↓
[ASP.NET Core App Container]  ──→  [SQL Server 2022 Container]
    (cptcevents-app)                    (sqlserver)
         └──────────────────────────────────┘
                      CptcEventsNetwork (Docker bridge)
```

All components run on the same self-hosted server. The app, database, and Cloudflare tunnel containers all communicate over a shared Docker bridge network (`CptcEventsNetwork`).

## Infrastructure Components

### Self-Hosted Server

The application and database both run directly on the production server as Docker containers. The GitHub Actions runner is also installed on this same machine, allowing it to build and deploy the app in place.

### SQL Server 2022 (Docker)

The database runs as a Docker container managed by `deploy/docker-compose.yml`. It uses a named volume (`sqlserver-data`) to persist data across container restarts and redeployments.

Database migrations are applied on app startup

**Key configuration:**
- Container name/hostname: `sqlserver`
- Port: `1433` (internal to `CptcEventsNetwork`)
- Data persistence: `sqlserver-data` Docker volume
- SA password: managed via `SA_PASSWORD` secret
- App DB user: `cptcevents_app` with password from `CPTCEVENTS_DB_PASSWORD` secret

### Cloudflare Tunnel

Provides secure public access to the server without exposing ports directly to the internet, handling DNS routing from `cptcevents.org`.

## CI/CD Pipeline

The application uses GitHub Actions with a **self-hosted runner** installed on the production server. This allows the runner to build Docker images and deploy containers directly on the same machine.

### Build Workflow

**Trigger**: Pull requests to `main` branch

**Purpose**: Validates that the code builds and passes tests

**Pipeline Stages**:

1. **Checkout & Setup** — Checkout source code and install .NET 10 Preview SDK
2. **Build & Test** — Restore NuGet dependencies, build in Release configuration, and run tests

### Deployment Workflow

**Trigger**: Pushes to `main` branch (after PR merge)

**Purpose**: Automatically builds and deploys the application to the production server.

**Pipeline Stages**:

1. **Checkout** — Pull the latest source code onto the runner

2. **Validate Secrets** — Verify all required GitHub secrets are present before taking any action

3. **Build Image** — Build the Docker image from `CptcEvents/Dockerfile`, tagging it with both the commit SHA and `latest`

4. **Start Infrastructure** — Run `docker compose -f deploy/docker-compose.yml up -d sqlserver cloudflared` to ensure the database and Cloudflare tunnel are running (leave alone if already healthy)

5. **Deploy App** — Run `docker compose -f deploy/docker-compose.yml up -d --force-recreate cptcevents-app` to recreate the app container with the new image (migrations run during app startup)

### Secrets Management

The pipeline uses GitHub Secrets to securely manage sensitive configuration:

| Secret | Purpose |
|---|---|
| `SA_PASSWORD` | SQL Server SA password (used to start the DB container) |
| `CPTCEVENTS_DB_PASSWORD` | Password for the `cptcevents_app` database user (used by the app container) |
| `CLOUDFLARE_TUNNEL_TOKEN` | Token for the Cloudflare tunnel container |
| `SENDGRID_API_KEY` | Email service API key |
| `ADMIN_EMAIL` | Initial admin account email |
| `ADMIN_USERNAME` | Initial admin account username |
| `ADMIN_PASSWORD` | Initial admin account password |
| `AZURE_BLOB_CONNECTION_STRING` | Azure Blob Storage for file uploads |

These secrets are injected as environment variables at deployment

## Additional Resources

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [CptcEvents Loading Page Repository](https://github.com/aiden-richard/CptcEvents-Loading-Page)

---

*For local development setup and troubleshooting, see [DEVELOPMENT.md](DEVELOPMENT.md).*
