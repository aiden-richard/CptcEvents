# CptcEvents

## Overview

An event management web application for Clover Park Technical College. Students, staff, and community members can discover, create, and manage campus events through an interactive calendar. The platform supports group-based event organization, role-based access control, and automated email notifications.

## Features

### Event Management
- Interactive calendar
- Create, edit, and delete events
- Public events for the community, private events for groups
- Admin approval system for homepage events

### Groups
- Create groups for classes, clubs, departments, or interests
- Role-based permissions: Owner, Moderator, and Member
- Invite members using secure codes
- Dedicated calendar for each group

### Authentication
- ASP.NET Core Identity
- Email verification using @cptc.edu addresses
- Special instructor registration codes
- Custom authorization policies for group roles

### Email
- SendGrid integration for email delivery
- Automated group invitations
- Account confirmation emails

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 10.0 (MVC)
- **Language**: C# 13
- **ORM**: Entity Framework Core 10.0 with automatic migrations
- **Authentication**: ASP.NET Core Identity
- **Authorization**: Policy-based with custom requirements and handlers

### Frontend
- **Template Engine**: Razor Views (MVC pattern)
- **CSS Framework**: Bootstrap 5
- **JavaScript**: FullCalendar interactive calendar

### Database & Infrastructure
- **Development**: SQL Server 2022 (Docker)
- **Production**: Azure SQL Database
- **Hosting**: Azure Container Apps (serverless)
- **CI/CD**: GitHub Actions

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for complete infrastructure details.

## Get Started

```bash
# Clone the repository
git clone https://github.com/aiden-richard/CptcEvents.git
cd CptcEvents

# Start SQL Server with Docker Compose
cd SqlDevServer
docker compose up -d

# Run the application (migrations run automatically)
cd ../CptcEvents
dotnet run
```

Open https://localhost:7274 or http://localhost:5000 and log in with `admin@cptc.edu` / `CptcDev`

See [DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed setup instructions.

## Deployment

GitHub Actions handles CI/CD with build validation on pull requests and automatic deployment to Azure Container Apps after merging to `main`.

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for infrastructure details.

## Project Structure

```
CptcEvents/
├── Application/
│   └── Mappers/
├── Areas/
│   └── Identity/
├── Authorization/
│   ├── Handlers/
│   └── Requirements/
├── Controllers/
├── Data/
├── Migrations/
├── Models/
├── Services/
├── ViewComponents/
├── Views/
└── wwwroot/

SqlDevServer/
docs/
```

## Documentation

- [DEVELOPMENT.md](docs/DEVELOPMENT.md) - Local setup and troubleshooting
- [DEPLOYMENT.md](docs/DEPLOYMENT.md) - Cloud architecture and CI/CD

## License

See [LICENSE.txt](LICENSE.txt)

---

Built for Clover Park Technical College
