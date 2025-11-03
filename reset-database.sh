#!/bin/bash

# Script to reset the database by dropping and recreating it
# This will delete all data and reapply all migrations

cd "$(dirname "$0")/Monopoly.Api"

echo "🗑️  Dropping database..."
dotnet ef database drop --context ApplicationDbContext --project ../Monopoly.Data/Monopoly.Data.csproj --force

echo ""
echo "🔄 Creating database and applying migrations..."
dotnet ef database update --context ApplicationDbContext --project ../Monopoly.Data/Monopoly.Data.csproj

echo ""
echo "✅ Database reset complete!"
