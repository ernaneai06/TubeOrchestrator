# Docker Deployment Guide

This guide explains how to run the entire TubeOrchestrator platform using Docker.

## Quick Start

### Prerequisites
- Docker Engine 20.10+
- Docker Compose 2.0+

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/TubeOrchestrator.git
cd TubeOrchestrator
```

### 2. Configure Environment Variables (Optional)
Create a `.env` file in the root directory:

```env
# AI Provider API Keys (leave default to use mock AI)
DEEPSEEK_API_KEY=your-deepseek-api-key-here
OPENAI_API_KEY=your-openai-api-key-here
```

### 3. Build and Run
```bash
# Build all images
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f
```

### 4. Access the Application
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **OpenAPI Docs**: http://localhost:5000/openapi/v1.json

### 5. Stop the Application
```bash
docker-compose down

# To also remove volumes (database and media files)
docker-compose down -v
```

## Architecture

The Docker setup consists of 3 main services:

### 1. tube-api (Server)
- **Image**: Built from `src/TubeOrchestrator.Server/Dockerfile`
- **Port**: 5000
- **Purpose**: REST API server with embedded background worker
- **Volumes**:
  - `/app/media` - Shared media files
  - `/app/data` - SQLite database

### 2. tube-worker (Background Worker)
- **Image**: Built from `src/TubeOrchestrator.Worker/Dockerfile`
- **Purpose**: Dedicated worker for video processing
- **Features**: Includes FFmpeg for video rendering
- **Volumes**:
  - `/app/media` - Shared media files (same as API)
  - `/app/data` - Shared database (same as API)

### 3. tube-web (Frontend)
- **Image**: Built from `src/TubeOrchestrator.Web/Dockerfile`
- **Port**: 3000 (mapped to 80 inside container)
- **Purpose**: React SPA served by Nginx

## Volume Management

### Media Files Volume
All generated videos, audio files, and images are stored in the `media-files` volume, which is shared between the API and Worker.

```bash
# Inspect media files
docker volume inspect tubeorchestrator_media-files

# Backup media files
docker run --rm -v tubeorchestrator_media-files:/data -v $(pwd):/backup alpine tar czf /backup/media-backup.tar.gz -C /data .

# Restore media files
docker run --rm -v tubeorchestrator_media-files:/data -v $(pwd):/backup alpine tar xzf /backup/media-backup.tar.gz -C /data
```

### Database Volume
The SQLite database is stored in the `db-data` volume.

```bash
# Backup database
docker run --rm -v tubeorchestrator_db-data:/data -v $(pwd):/backup alpine cp /data/tubeorchestrator.db /backup/

# Restore database
docker run --rm -v tubeorchestrator_db-data:/data -v $(pwd):/backup alpine cp /backup/tubeorchestrator.db /data/
```

## Configuration

### Using Real AI Providers
Edit `docker-compose.yml` and set `UseMockAI=false`:

```yaml
tube-api:
  environment:
    - UseMockAI=false
    - DeepSeek__ApiKey=${DEEPSEEK_API_KEY}
```

Then provide your API key in the `.env` file.

### Switching to PostgreSQL
Uncomment the PostgreSQL service in `docker-compose.yml` and update connection strings:

```yaml
postgres:
  image: postgres:16-alpine
  # ... (uncomment the full section)

tube-api:
  environment:
    - ConnectionStrings__DefaultConnection=Host=postgres;Database=tubeorchestrator;Username=tubeuser;Password=tubepass123
  depends_on:
    - postgres
```

## Troubleshooting

### FFmpeg Issues in Worker
If video rendering fails, check FFmpeg is installed:
```bash
docker exec -it tube-worker ffmpeg -version
```

### Database Locked Errors
If you see database locked errors, ensure only one instance is writing:
```bash
docker-compose down
docker-compose up -d
```

### Port Conflicts
If ports 3000 or 5000 are in use, edit `docker-compose.yml`:
```yaml
ports:
  - "8000:5000"  # API
  - "8080:80"    # Frontend
```

### Viewing Container Logs
```bash
# All containers
docker-compose logs -f

# Specific container
docker-compose logs -f tube-api
docker-compose logs -f tube-worker
docker-compose logs -f tube-web
```

### Rebuilding After Code Changes
```bash
# Rebuild specific service
docker-compose build tube-api

# Rebuild all services
docker-compose build

# Rebuild and restart
docker-compose up -d --build
```

## Production Deployment

### Security Recommendations
1. **Change default passwords** in `docker-compose.yml`
2. **Use secrets** for API keys instead of environment variables
3. **Enable HTTPS** using a reverse proxy (Nginx/Caddy)
4. **Set resource limits** for containers
5. **Regular backups** of volumes

### Example with Resource Limits
```yaml
tube-worker:
  deploy:
    resources:
      limits:
        cpus: '2.0'
        memory: 4G
      reservations:
        cpus: '1.0'
        memory: 2G
```

### Using Docker Swarm or Kubernetes
For production scale, consider:
- Docker Swarm for simpler orchestration
- Kubernetes for advanced scaling and management
- Use external storage (S3, NFS) for media files
- Separate database server (PostgreSQL cluster)

## Health Checks

All services include health checks:

```bash
# Check service health
docker-compose ps

# Manual health check
curl http://localhost:5000/health
```

## Maintenance

### Cleanup Unused Resources
```bash
# Remove stopped containers
docker-compose down

# Remove unused images
docker image prune -a

# Remove unused volumes (CAUTION: this deletes data!)
docker volume prune
```

### Update to Latest Version
```bash
git pull
docker-compose down
docker-compose build
docker-compose up -d
```

## Support

For issues or questions:
- Check container logs: `docker-compose logs`
- Open an issue on GitHub
- Review the main README.md for architecture details
