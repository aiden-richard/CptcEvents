-- Create application login (idempotent)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'cptcevents_app')
BEGIN
    CREATE LOGIN cptcevents_app WITH PASSWORD = '$(CPTCEVENTS_DB_PASSWORD)';
END
GO

-- Allow the app login to create the production database on first deploy
ALTER SERVER ROLE dbcreator ADD MEMBER cptcevents_app;
GO
