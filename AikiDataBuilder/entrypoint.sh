#!/bin/sh
set -e

echo "📦 Applying EF Core migrations..."
dotnet ef database update --no-build --project AikiDataBuilder/AikiDataBuilder.csproj --startup-project AikiDataBuilder/AikiDataBuilder.csproj

echo "🚀 Starting application..."
exec dotnet AikiDataBuilder.dll
