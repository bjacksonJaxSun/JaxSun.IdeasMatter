# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Jackson.Ideas.Api/Jackson.Ideas.Api.csproj", "Jackson.Ideas.Api/"]
COPY ["src/Jackson.Ideas.Application/Jackson.Ideas.Application.csproj", "Jackson.Ideas.Application/"]
COPY ["src/Jackson.Ideas.Core/Jackson.Ideas.Core.csproj", "Jackson.Ideas.Core/"]
COPY ["src/Jackson.Ideas.Infrastructure/Jackson.Ideas.Infrastructure.csproj", "Jackson.Ideas.Infrastructure/"]
COPY ["src/Jackson.Ideas.Shared/Jackson.Ideas.Shared.csproj", "Jackson.Ideas.Shared/"]

RUN dotnet restore "Jackson.Ideas.Api/Jackson.Ideas.Api.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Jackson.Ideas.Api"
RUN dotnet build "Jackson.Ideas.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Jackson.Ideas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install SQLite
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create directory for SQLite database and ensure it's writable
RUN mkdir -p /app/data && chmod 777 /app/data
RUN mkdir -p /app && chmod 777 /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Use PORT environment variable from Render
ENV PORT=8080

EXPOSE 8080

# Create a startup script to handle port binding
RUN echo '#!/bin/bash\nexport ASPNETCORE_URLS="http://+:${PORT:-8080}"\nexec dotnet Jackson.Ideas.Api.dll' > /app/start.sh
RUN chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]