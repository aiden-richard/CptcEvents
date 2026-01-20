# CPTC Events

## Overview

CPTC Events is an event management web application designed for Clover Park Technical College. The platform features an interactive calendar that allows students, staff, and community members to discover, create, and manage events efficiently. The application supports group-based event organization and provides both public and authenticated access levels.

## Features

### Event Management
- **Interactive Calendar**: FullCalendar-based interface with sorting and filtering capabilities
- **Event CRUD Operations**: Create, read, update, and delete events with rich details
- **Public & Private Events**: Control event visibility for different user groups
- **Event Approval System**: Events shown on the homepage are approved by Admin

### Group Management
- **Group Creation**: Create groups for your class, club, and more
- **Role-Based Access**: Owner, Moderator, and Member roles with different permissions
- **Group Invitations**: Invite members via invite codes
- **Group-Specific Calendars**: See a group's events on its dedicated calendar

### User Authentication
- **ASP.NET Core Identity**: Secure authentication and authorization
- **Email Verification**: Verified CPTC accounts using institutional email addresses
- **Instructor Codes**: Special registration codes for verified instructors
- **Admin Panel**: Site Moderation for Admin

### Email Integration
- **SendGrid Integration**: Professional email delivery for notifications
- **Invitation Emails**: Automated group invitation system
- **Account Verification**: Email-based account confirmation

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 10.0
- **Language**: C# 13
- **ORM**: Entity Framework Core 10.0
- **Authentication**: ASP.NET Core Identity
- **Authorization**: Policy-based with custom requirements and handlers

### Frontend
- **Template Engine**: Razor Views (MVC pattern)
- **CSS Framework**: Bootstrap 5
- **Calendar**: FullCalendar JavaScript library
- **Icons**: Bootstrap Icons

### Database
- **Development**: SQL Server on Docker
- **Production**: Azure SQL Database (SQL Server)
- **Migrations**: EF Core migrations with automatic deployment

### Infrastructure
- **Hosting**: Azure Container Apps
- **Container Registry**: Azure Container Registry (ACR)
- **CI/CD**: GitHub Actions
- **Email Service**: SendGrid

## Architecture

The application follows the MVC (Model-View-Controller) pattern with additional layers:

- **Controllers**: Handle HTTP requests and coordinate business logic
- **Services**: Encapsulate business logic (EventService, GroupService, InviteService)
- **Authorization**: Policy-based authorization with custom handlers for group roles
- **Mappers**: Transform between models and view models
- **View Components**: Reusable UI components (CalendarViewComponent)
- **Data Layer**: Entity Framework Core with ApplicationDbContext

## Local Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (version 10.0.100-rc.2.25502.107 or later)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for running SQL Server locally)
- [SendGrid Account](https://sendgrid.com/) (for email functionality)

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/aiden-richard/CptcEvents.git
   cd CptcEvents
   ```

2. **Configure User Secrets**
   
   Navigate to the project directory and initialize user secrets:
   ```bash
   cd CptcEvents
   dotnet user-secrets init
   ```

   Start SQL Server in Docker:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
     -p 1433:1433 --name sqlserver --hostname sqlserver \
     -d mcr.microsoft.com/mssql/server:2022-latest
   ```

   Set required secrets:
   ```bash
   # Database connection (SQL Server running in Docker)
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CptcEvents;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
   
   # SendGrid API key (get from https://app.sendgrid.com/settings/api_keys)
   dotnet user-secrets set "SendGrid:ApiKey" "your-sendgrid-api-key"
   
   # Initial admin password
   dotnet user-secrets set "AdminUser:Password" "YourSecurePassword123!"
   ```

3. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```
   
   The application will be available at `http://localhost:5000` (or the port specified in `launchSettings.json`)

5. **Access the Application**
   - Navigate to `http://localhost:5000`
   - Register a new account or log in with admin credentials
   - The admin account is created automatically on first run using the password from user secrets

## Project Structure

```
CptcEvents/
├── Application/              # Application layer components
│   └── Mappers/             # Model-ViewModel mappers
├── Areas/
│   └── Identity/            # ASP.NET Identity customization
├── Authorization/           # Authorization policies and handlers
│   ├── Handlers/           # Custom authorization handlers
│   └── Requirements/       # Custom authorization requirements
├── Controllers/            # MVC controllers
├── Data/                   # Database context
├── Migrations/             # EF Core migrations
├── Models/                 # Domain models and ViewModels
├── Services/               # Business logic services
├── ViewComponents/         # Reusable view components
├── Views/                  # Razor views
└── wwwroot/                # Static files (CSS, JS, images)
```


### Adding Database Migrations

When you modify models or the database schema:

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName

# Review the generated migration file in Migrations/

# Apply to local database
dotnet ef database update
```

Migrations are automatically applied to production on deployment.

## Deployment

The application uses GitHub Actions for CI/CD with two workflows:

### Build Workflow (`.github/workflows/build.yml`)
- Triggers on pull requests to `main`
- Validates code compilation and tests
- Provides quick feedback before merging

### Deploy Workflow (`.github/workflows/deploy.yml`)
- Triggers on pushes to `main`
- Builds Docker image
- Pushes to Azure Container Registry
- Deploys to Azure Container Apps

For detailed deployment documentation, infrastructure setup, and troubleshooting, see [DEPLOYMENT.md](docs/DEPLOYMENT.md).

## Configuration

### Environment Variables (Production)

The following environment variables are configured in Azure Container Apps:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection` - Azure SQL connection string
- `SendGrid__ApiKey` - SendGrid API key for email
- `AdminUser__Password` - Initial admin user password

### User Secrets (Development)

Local development uses user secrets instead of environment variables:
- `ConnectionStrings:DefaultConnection`
- `SendGrid:ApiKey`
- `AdminUser:Password`

## License

This project is licensed under the terms specified in [LICENSE.txt](LICENSE.txt).

## Support

For issues, questions, or contributions:
- Open an issue on GitHub
- Contact the development team
- Review the [deployment documentation](docs/DEPLOYMENT.md) for infrastructure questions
