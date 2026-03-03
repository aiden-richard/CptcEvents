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
[Cloudflare Tunnel]
        ↓
[Caddy Reverse Proxy]
        ↓
[ASP.NET Core App Container]  ──→  [SQL Server 2022 Container]
    (cptcevents-app)                    (sqlserver)
         └──────────────────────────────────┘
                  sqldevserver_default Docker network
```

All components run on the same self-hosted server. The app and database containers communicate over a shared Docker network (`sqldevserver_default`) using the `sqlserver` hostname.

## Infrastructure Components

### Self-Hosted Server

The application and database both run directly on the production server as Docker containers. The GitHub Actions runner is also installed on this same machine, allowing it to build and deploy the app in place.

### SQL Server 2022 (Docker)

The database runs as a Docker container managed by `SqlDevServer/docker-compose.yml`. It uses a named volume (`sqlserver-data`) to persist data across container restarts and redeployments.

The database is managed through Entity Framework Core migrations, which are applied automatically on application startup via `context.Database.MigrateAsync()` in `Program.cs`.

**Key configuration:**
- Container name/hostname: `sqlserver`
- Port: `1433` (mapped to host)
- Data persistence: `sqlserver-data` Docker volume
- SA password: managed via `SA_PASSWORD` GitHub secret / `.env` file

### Caddy Reverse Proxy

Caddy sits in front of the application container and handles HTTPS termination and routing. It receives traffic forwarded by the Cloudflare Tunnel.

### Cloudflare Tunnel

Provides secure public access to the server without exposing ports directly to the internet, handling DNS routing from `cptcevents.org`.

## CI/CD Pipeline

The application uses GitHub Actions with a **self-hosted runner** installed on the production server. This allows the runner to build Docker images and deploy containers directly on the same machine.

### Build Workflow

**Trigger**: Pull requests to `main` branch

**Purpose**: Validates code quality before merging by ensuring the application compiles successfully.

**Pipeline Stages**:

1. **Checkout & Setup** — Checkout source code and install .NET 10 Preview SDK
2. **Build & Test** — Restore NuGet dependencies, build in Release configuration, and run tests

### Deployment Workflow

**Trigger**: Pushes to `main` branch (after PR merge)

**Purpose**: Automatically builds and deploys the application to the production server.

**Pipeline Stages**:

1. **Checkout** — Pull the latest source code onto the runner

2. **Start Database** — Run `docker compose up -d` in `SqlDevServer/` to ensure the SQL Server container is running (no-op if already up)

3. **Build Image** — Build the Docker image from `CptcEvents/Dockerfile`, tagging it with both the commit SHA and `latest`

4. **Replace Container** — Stop and remove the existing `cptcevents-app` container, then start a new one on the `sqldevserver_default` network with all required environment variables injected

### Secrets Management

The pipeline uses GitHub Secrets to securely manage sensitive configuration:

| Secret | Purpose |
|---|---|
| `SA_PASSWORD` | SQL Server SA password (used to start the DB container and build the app connection string) |
| `SENDGRID_API_KEY` | Email service API key |
| `ADMIN_EMAIL` | Initial admin account email |
| `ADMIN_USERNAME` | Initial admin account username |
| `ADMIN_PASSWORD` | Initial admin account password |
| `AZURE_BLOB_CONNECTION_STRING` | Azure Blob Storage for file uploads |

These secrets are injected as environment variables into the running container at deployment time. The database connection string is constructed from `SA_PASSWORD` at deploy time — no separate `SQL_CONNECTION_STRING` secret is needed.

## Additional Resources

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [CptcEvents Loading Page Repository](https://github.com/aiden-richard/CptcEvents-Loading-Page)

---

*For local development setup and troubleshooting, see [DEVELOPMENT.md](DEVELOPMENT.md).*
