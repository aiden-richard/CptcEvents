# Improving CI/CD Workflows for CptcEvents

## Objective
Automate deployment to Azure Container Instances on every push to `main`, replacing manual container building and server management.

## Decisions Made
- **Database:** Move from SQLite to Azure SQL Database (SQL Server) for simplicity with EF Core and better compatibility
- **Hosting:** Azure Container Instances (simpler container running, no Kubernetes complexity)
- **CI/CD Platform:** GitHub Actions (already partially configured)
- **Monitoring:** Azure Cost Alerts to prevent unexpected charges

## Implementation Roadmap (Best Order)

### Phase 0: Prep
- [x] Create a feature branch for DB migration (e.g., `feat/sqlserver-migration`)
- [x] Optional: Export any existing SQLite data that you want to keep

### Phase 1: Codebase Switch to SQL Server
- [x] Replace EF Core provider package to `Microsoft.EntityFrameworkCore.SqlServer`
- [x] Update `Program.cs` to use `UseSqlServer()` and read `DefaultConnection` from configuration/environment
- [x] Add a production connection string template in `appsettings.json` (without secrets)
- [x] Verify `CptcEvents.csproj` includes EF tools packages for migrations

### Phase 2: Generate SQL Server Migration
- [x] Run `dotnet ef migrations add SqlServerMigration` to snapshot the current model for SQL Server
- [x] Inspect the generated migration to ensure types/indexes look correct

### Phase 3: Provision Azure SQL Database
- [x] Create Azure resource group
- [x] Create Azure SQL Server + Azure SQL Database
- [x] Configure firewall rules to allow CI/CD and your local IP
- [x] Retrieve the ADO.NET connection string

### Phase 4: Local Verification Against SQL Server
- [x] Temporarily set `appsettings.Development.json` (or environment variable) to point to Azure SQL (or local SQL Server Express)
- [x] Run `dotnet ef database update` to apply migrations
- [x] Run the app locally and validate end-to-end behavior

### Phase 5: Azure Resources for Container Hosting
- [x] Create Azure Container Registry (ACR)
- [ ] Create Azure Container Instances (ACI) for hosting the container
- [ ] Optional: Create Azure Key Vault for secrets management
- [ ] Set up Azure Cost Alerts on the subscription/resource group

### Phase 6: CI/CD Pipeline Enhancement
- [ ] Configure GitHub Secrets: Azure credentials, SQL connection string, SendGrid API key, admin password
- [ ] Update `.github/workflows/dotnet.yml` to build the Docker image
- [ ] Push image to ACR
- [ ] Deploy to ACI using the latest image
- [ ] Automate EF Core migrations (startup migration or a deploy step)
- [ ] Test the workflow on the `main` branch

### Phase 7: Documentation & Validation
- [ ] Document the deployment process and rollback steps
- [ ] Create a small runbook for manual interventions
- [ ] Validate cost alerts are triggering as expected
- [ ] Add basic health checks and monitoring

## Current State
- GitHub Actions workflow exists but only builds the project
- Dockerfile is production-ready with multi-stage build
- SQLite database with comprehensive seeding
- User Secrets infrastructure in place
- No Azure infrastructure or deployment automation

## Next Steps
Start with Phase 1–4 to complete the SQL Server switch and local verification. Then proceed with Phase 5–6 to enable CI/CD and Azure hosting, followed by Phase 7 for documentation and monitoring.
