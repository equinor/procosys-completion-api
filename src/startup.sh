#!/bin/bash

# Run database migrations
echo "Running database migrations..."

/app/migrations/bundle.exe --connection "$DB_CONNECTION_STRING"

# Check if migrations succeeded
if [ $? -ne 0 ]; then
  echo "Database migrations failed. Exiting."
  exit 1
fi

# Start the main application
echo "Starting the application..."
dotnet /app/Equinor.ProCoSys.Completion.WebApi.dll
