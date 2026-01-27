#!/bin/bash
set -e

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to start (up to 60 seconds)
echo "Waiting for SQL Server to start..."
for i in {1..60}; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "SQL Server started successfully"
        break
    fi
    echo "Waiting... ($i/60)"
    sleep 1
done

# Run initialization script
if [ -f /docker-entrypoint-initdb.d/init.sql ]; then
    echo "Running initialization script..."
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /docker-entrypoint-initdb.d/init.sql
    echo "Initialization complete"
fi

# Keep the container running
wait
