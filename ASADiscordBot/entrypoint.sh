#!/bin/sh
set -e

echo "📦 Applying EF Core migrations..."
dotnet ef database update --no-build --project ASADiscordBot/ASADiscordBot.csproj --startup-project ASADiscordBot/ASADiscordBot.csproj

echo "🚀 Starting application..."
exec dotnet ASADiscordBot.dll
