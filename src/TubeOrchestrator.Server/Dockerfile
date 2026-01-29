# Multi-stage build for TubeOrchestrator API Server
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/TubeOrchestrator.Server/TubeOrchestrator.Server.csproj", "TubeOrchestrator.Server/"]
COPY ["src/TubeOrchestrator.Core/TubeOrchestrator.Core.csproj", "TubeOrchestrator.Core/"]
COPY ["src/TubeOrchestrator.Data/TubeOrchestrator.Data.csproj", "TubeOrchestrator.Data/"]
COPY ["src/TubeOrchestrator.Infrastructure/TubeOrchestrator.Infrastructure.csproj", "TubeOrchestrator.Infrastructure/"]
COPY ["src/TubeOrchestrator.Worker/TubeOrchestrator.Worker.csproj", "TubeOrchestrator.Worker/"]

# Restore dependencies
RUN dotnet restore "TubeOrchestrator.Server/TubeOrchestrator.Server.csproj"

# Copy all source code
COPY src/ .

# Build and publish
WORKDIR "/src/TubeOrchestrator.Server"
RUN dotnet build "TubeOrchestrator.Server.csproj" -c Release -o /app/build
RUN dotnet publish "TubeOrchestrator.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Expose port
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Copy published files
COPY --from=build /app/publish .

# Create volume mount point for shared media and database
VOLUME ["/app/media", "/app/data"]

ENTRYPOINT ["dotnet", "TubeOrchestrator.Server.dll"]
