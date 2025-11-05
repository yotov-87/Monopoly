# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Monopoly.sln ./
COPY Monopoly.Api/Monopoly.Api.csproj Monopoly.Api/
COPY Monopoly.Core/Monopoly.Core.csproj Monopoly.Core/
COPY Monopoly.Data/Monopoly.Data.csproj Monopoly.Data/
COPY Monopoly.Hubs/Monopoly.Hubs.csproj Monopoly.Hubs/

# Restore dependencies
RUN dotnet restore Monopoly.Api/Monopoly.Api.csproj

# Copy all source files
COPY . .

# Build and publish
WORKDIR /src/Monopoly.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port (Render uses port 10000 by default)
EXPOSE 10000

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:10000

# Run the application
ENTRYPOINT ["dotnet", "Monopoly.Api.dll"]
