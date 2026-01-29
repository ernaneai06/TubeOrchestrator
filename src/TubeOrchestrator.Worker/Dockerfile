# Multi-stage build for TubeOrchestrator Worker with FFmpeg
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/TubeOrchestrator.Worker/TubeOrchestrator.Worker.csproj", "TubeOrchestrator.Worker/"]
COPY ["src/TubeOrchestrator.Core/TubeOrchestrator.Core.csproj", "TubeOrchestrator.Core/"]
COPY ["src/TubeOrchestrator.Data/TubeOrchestrator.Data.csproj", "TubeOrchestrator.Data/"]
COPY ["src/TubeOrchestrator.Infrastructure/TubeOrchestrator.Infrastructure.csproj", "TubeOrchestrator.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TubeOrchestrator.Worker/TubeOrchestrator.Worker.csproj"

# Copy all source code
COPY src/ .

# Build and publish
WORKDIR "/src/TubeOrchestrator.Worker"
RUN dotnet build "TubeOrchestrator.Worker.csproj" -c Release -o /app/build
RUN dotnet publish "TubeOrchestrator.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage with FFmpeg
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install FFmpeg and other dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Verify FFmpeg installation
RUN ffmpeg -version

# Copy published files
COPY --from=build /app/publish .

# Create volume mount points for shared media and database
VOLUME ["/app/media", "/app/data"]

ENTRYPOINT ["dotnet", "TubeOrchestrator.Worker.dll"]
