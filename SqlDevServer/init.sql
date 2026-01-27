-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CptcEvents-Sql-DevServer')
BEGIN
    CREATE DATABASE [CptcEvents-Sql-DevServer];
END
GO
