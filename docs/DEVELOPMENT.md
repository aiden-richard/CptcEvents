# Local Development Guide

This guide provides detailed instructions for setting up the CptcEvents application for local development.

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (version 10.0.100-rc.2.25502.107 or later)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for running SQL Server locally)
- [SendGrid Account](https://sendgrid.com/) (for email functionality)

## Quick Start

1. Clone the repository
2. Start SQL Server with Docker Compose
3. Run the application (migrations run automatically)

## Detailed Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/aiden-richard/CptcEvents.git
cd CptcEvents
```

### 2. Start SQL Server with Docker Compose

The application uses SQL Server for data storage. A Docker Compose configuration is provided for easy setup:

```bash
cd SqlDevServer
docker compose up -d
```

This will:
- Start SQL Server 2022 on port 1433
- Create the `CptcEvents-Sql-DevServer` database automatically
- Persist data in a Docker volume (survives restarts)
- Use SA password from `.env` file (`CptcDev123!`)

**Verify SQL Server is running:**
```bash
docker compose ps
```

You should see the `sqlserver` container with status "Up".

**Common Docker Compose Commands:**
```bash
# Stop SQL Server (data persists)
docker compose stop

# Start SQL Server (after stopping)
docker compose start

# View SQL Server logs
docker compose logs -f

# Restart SQL Server
docker compose restart

# Stop and remove container (data persists in volume)
docker compose down

# Stop and remove everything including data
docker compose down -v
```

### 3. Configuration (Optional)

**Development settings are pre-configured** in `appsettings.Development.json`:
- Database: `CptcEvents-Sql-DevServer` on localhost:1433
- Admin credentials: `admin@cptc.edu` / `CptcDev`
- SA password: `CptcDev123!` (from `SqlDevServer/.env`)

**User Secrets (Optional - for production-like testing):**

If you need to override development settings or test with production services:

```bash
cd CptcEvents

# Override connection string (e.g., for Azure SQL testing)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

# Add SendGrid API key for email testing
dotnet user-secrets set "SendGrid:ApiKey" "your-sendgrid-api-key"

# Override admin password
dotnet user-secrets set "AdminUser:Password" "YourSecurePassword123!"
```

**Note:** User secrets take precedence over `appsettings.Development.json`.

### 4. Run the Application

```bash
cd CptcEvents
dotnet run
```

The application will:
- Automatically apply pending migrations
- Create the database if it doesn't exist
- Seed the admin user and default data
- Start on `https://localhost:7134` (HTTPS) and `http://localhost:5000` (HTTP)

### 5. Access the Application

- Navigate to `https://localhost:7134` or `http://localhost:5000`
- Log in with admin credentials: `admin@cptc.edu` / `CptcDev`
- Register new accounts or create events/groups

**If you encounter database connection issues:**
- Ensure SQL Server container is running: `docker compose ps` (in SqlDevServer/)
- Check Docker logs: `docker compose logs sqlserver`
- Verify port 1433 is not in use: `lsof -i :1433`

## Development Workflow

### Adding Database Migrations

When you modify models or the database schema:

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName

# Review the generated migration file in Migrations/

# Apply to local database
dotnet ef database update
```

**Migration Best Practices:**
- Use descriptive migration names (e.g., `AddEventLocationField`, `CreateGroupInvitesTable`)
- Review generated migration code before applying
- Test migrations locally before pushing to main branch
- Migrations are automatically applied to production on deployment

### Running with HTTPS

HTTPS is enabled by default. The application runs on:
- `https://localhost:7134` (HTTPS - default)
- `http://localhost:5000` (HTTP)

To run HTTP only:
```bash
dotnet run --launch-profile http
```

## IDE Setup

### Visual Studio 2022

1. Open `CptcEvents.slnx` in Visual Studio
2. Set `CptcEvents` as the startup project
3. Press F5 to run with debugging

### VS Code

1. Install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension
2. Open the `CptcEvents` folder
3. Press F5 to run with debugging (select ".NET Core" if prompted)

**Recommended VS Code Extensions:**
- C# Dev Kit
- C#
- NuGet Package Manager
- Docker

## Additional Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Docker Documentation](https://docs.docker.com/)
- [SendGrid Documentation](https://docs.sendgrid.com/)
- [Deployment Guide](DEPLOYMENT.md) - For production deployment instructions
