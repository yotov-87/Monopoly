# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Monopoly.Api/Monopoly.Api.csproj", "Monopoly.Api/"]
COPY ["Monopoly.Core/Monopoly.Core.csproj", "Monopoly.Core/"]
COPY ["Monopoly.Data/Monopoly.Data.csproj", "Monopoly.Data/"]
COPY ["Monopoly.Hubs/Monopoly.Hubs.csproj", "Monopoly.Hubs/"]
RUN dotnet restore "Monopoly.Api/Monopoly.Api.csproj"

# Copy all source files
COPY . .
WORKDIR "/src/Monopoly.Api"
RUN dotnet build "Monopoly.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Monopoly.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Monopoly.Api.dll"]
