-- Create dev database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CptcEvents-Sql-DevServer')
BEGIN
    CREATE DATABASE [CptcEvents-Sql-DevServer];
END
GO

-- Create application login (idempotent)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'cptcevents_app')
BEGIN
    CREATE LOGIN cptcevents_app WITH PASSWORD = '$(CPTCEVENTS_DB_PASSWORD)';
END
GO

-- Grant login access to dev database
USE [CptcEvents-Sql-DevServer];
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'cptcevents_app')
BEGIN
    CREATE USER cptcevents_app FOR LOGIN cptcevents_app;
    ALTER ROLE db_owner ADD MEMBER cptcevents_app;
END
GO
