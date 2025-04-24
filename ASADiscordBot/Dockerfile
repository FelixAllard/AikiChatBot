# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=0
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5173

# Build and publish stage with SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/ASADiscordBot

# Copy csproj and restore
COPY ASADiscordBot/ASADiscordBot.csproj ./
RUN dotnet restore "ASADiscordBot.csproj"

# Install EF CLI
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy rest of the source
COPY ASADiscordBot/. ./

# ✅ Build the project first
RUN dotnet build "ASADiscordBot.csproj" -c $BUILD_CONFIGURATION

# ✅ Now apply migrations (the deps.json exists now)
RUN dotnet ef database update --project ASADiscordBot.csproj --startup-project ASADiscordBot.csproj


# Build and publish
RUN dotnet publish "ASADiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image: only ASP.NET + published app
FROM base AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish ./

# Set entrypoint
ENTRYPOINT ["dotnet", "ASADiscordBot.dll"]
