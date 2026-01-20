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
2. Start SQL Server in Docker
3. Configure user secrets
4. Run database migrations
5. Start the application

## Detailed Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/aiden-richard/CptcEvents.git
cd CptcEvents
```

### 2. Start SQL Server in Docker

The application uses SQL Server for data storage. Run the following command to start a SQL Server container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver --hostname sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**Verify SQL Server is running:**
```bash
docker ps
```

You should see the `sqlserver` container in the list with status "Up".

**Common Docker Commands:**
```bash
# Stop SQL Server
docker stop sqlserver

# Start SQL Server (after stopping)
docker start sqlserver

# Remove SQL Server container (data will be lost)
docker rm sqlserver

# View SQL Server logs
docker logs sqlserver
```

### 3. Configure User Secrets

Navigate to the project directory and initialize user secrets:

```bash
cd CptcEvents
dotnet user-secrets init
```

Set the required secrets:

```bash
# Database connection (SQL Server running in Docker)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CptcEvents;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"

# SendGrid API key (get from https://app.sendgrid.com/settings/api_keys)
dotnet user-secrets set "SendGrid:ApiKey" "your-sendgrid-api-key"

# Initial admin password
dotnet user-secrets set "AdminUser:Password" "YourSecurePassword123!"
```

**View configured secrets:**
```bash
dotnet user-secrets list
```

### Required Secrets Reference

| Secret Name | Description | How to Obtain |
|-------------|-------------|---------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Use the Docker SQL Server connection string above, or connect to Azure SQL for testing against production data |
| `SendGrid:ApiKey` | API key for sending emails | [SendGrid Dashboard](https://app.sendgrid.com/settings/api_keys) → Create API Key |
| `AdminUser:Password` | Password for the initial admin account | Choose a secure password (min 8 chars, uppercase, lowercase, number, special char) |

### 4. Apply Database Migrations

Create the database and apply all migrations:

```bash
dotnet ef database update
```

**If you encounter issues:**
- Ensure SQL Server container is running (`docker ps`)
- Verify the connection string in user secrets (`dotnet user-secrets list`)
- Check that port 1433 is not in use by another application

### 5. Run the Application

```bash
dotnet run
```

The application will be available at `http://localhost:5000` (or the port specified in `Properties/launchSettings.json`).

### 6. Access the Application

- Navigate to `http://localhost:5000`
- Register a new account or log in with admin credentials
- The admin account (`admin@cptc.edu`) is created automatically on first run using the password from user secrets

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

### Running with HTTPS (Optional)

To run with HTTPS locally:

```bash
dotnet run --launch-profile https
```

The application will be available at:
- `https://localhost:7274` (HTTPS)
- `http://localhost:5000` (HTTP)


Changes to Razor views, CSS, and C# code will automatically reload.

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
