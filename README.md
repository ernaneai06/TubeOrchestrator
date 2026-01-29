# TubeOrchestrator

A modern SaaS platform for automating YouTube channel content creation and management.

## Architecture Overview

This project implements a **Modular Monolithic Architecture** using .NET 10 and React, designed to manage multiple YouTube channels with automated video generation.

### Tech Stack

**Backend:**
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core with SQLite
- System.Threading.Channels for job queue
- Background Worker Service

**Frontend:**
- React 19 with Vite
- TailwindCSS for styling
- Axios for API communication
- Live job monitoring with polling

### Project Structure

```
/src
  /TubeOrchestrator.Core        # Domain entities, interfaces, business logic
  /TubeOrchestrator.Data        # EF Core DbContext, repositories
  /TubeOrchestrator.Server      # Web API, controllers, SignalR (future)
  /TubeOrchestrator.Worker      # Background service for video processing
  /TubeOrchestrator.Web         # React frontend
```

## Key Features

### 1. Multi-Channel Management
- Support for multiple platforms (YouTube, TikTok)
- Channel-specific configuration and scheduling
- Active/Inactive channel states

### 2. Niche-Based Content
- Organize channels by niche (Tech News, Meditation, etc.)
- Dynamic prompt templates per niche
- Template variable substitution ({{NEWS_DATA}}, {{TOPIC}})

### 3. Producer-Consumer Job Queue
- Non-blocking API using `System.Threading.Channels`
- Background worker processes jobs independently
- Prevents system overload during video rendering

### 4. Video Generation Pipeline
The `VideoGenerationService` implements a three-step pipeline:
1. **FetchContent**: Retrieve news/content based on channel niche
2. **GenerateScript**: Use AI prompt templates to create video scripts
3. **RenderVideo**: Generate and upload videos to the platform

### 5. Real-Time Monitoring
- Dashboard shows all channels with status
- Live job queue with auto-refresh (5-second polling)
- Job status tracking (Pending → Processing → Completed/Failed)

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- npm or yarn

### Backend Setup

1. **Build the solution:**
   ```bash
   cd /path/to/TubeOrchestrator
   dotnet build
   ```

2. **Run the API Server (includes Worker):**
   ```bash
   cd src/TubeOrchestrator.Server
   dotnet run
   ```
   
   The server will start on `http://localhost:5165`
   
   On first run, it will:
   - Create the SQLite database
   - Seed sample data (2 niches, 3 channels, prompt templates)
   - Start the background worker

### Frontend Setup

1. **Install dependencies:**
   ```bash
   cd src/TubeOrchestrator.Web
   npm install
   ```

2. **Start the development server:**
   ```bash
   npm run dev
   ```
   
   The React app will start on `http://localhost:5173`

### Testing the Workflow

1. Open the dashboard at `http://localhost:5173`
2. You'll see seeded channels (Tech Daily, Mindful Moments, Tech Shorts)
3. Click "Trigger Now" on an active channel
4. Watch the job status change in real-time in the Jobs Monitor table
5. The worker will simulate video generation (5 seconds) and mark it as completed

## API Endpoints

### Channels
- `GET /api/channels` - List all channels
- `GET /api/channels/active` - List active channels only
- `GET /api/channels/{id}` - Get channel by ID
- `POST /api/channels` - Create new channel
- `PUT /api/channels/{id}` - Update channel
- `DELETE /api/channels/{id}` - Delete channel

### Jobs
- `GET /api/jobs` - List all jobs
- `GET /api/jobs/recent?count=10` - Get recent jobs
- `GET /api/jobs/{id}` - Get job by ID
- `POST /api/jobs/trigger/{channelId}` - Trigger a new job for a channel

## Database Schema

### Tables
- **Niches**: Content categories (Tech News, Meditation, etc.)
- **Channels**: YouTube/TikTok channels with platform credentials
- **PromptTemplates**: AI prompts for script/title/description generation
- **Jobs**: Video generation tasks with status tracking

### Relationships
- `Niche` → `Channels` (1:Many)
- `Niche` → `PromptTemplates` (1:Many)
- `Channel` → `Jobs` (1:Many)

## Future Enhancements

### Phase 1 (Current)
- ✅ Foundation architecture
- ✅ Job queue system
- ✅ Basic video generation pipeline
- ✅ React dashboard

### Phase 2 (Planned)
- [ ] Integrate real AI APIs (OpenAI, Claude)
- [ ] Implement actual video rendering (FFmpeg, MoviePy)
- [ ] YouTube API integration for uploads
- [ ] Scheduled job execution (cron support)

### Phase 3 (Future)
- [ ] SignalR for real-time updates (replace polling)
- [ ] User authentication and multi-tenancy
- [ ] Analytics and performance metrics
- [ ] Content moderation and approval workflow

## Development Notes

### Why Modular Monolith?
- Easier to develop and deploy initially
- Shared in-memory queue (no Redis needed for POC)
- Can be split into microservices later if needed

### Design Decisions
1. **SQLite**: Lightweight, serverless, perfect for local dev
2. **Channels Pattern**: Built-in .NET, no external dependencies
3. **EF Core**: Type-safe queries, migrations, and relationship management
4. **TailwindCSS**: Utility-first, rapid UI development

### Testing Strategy
- Unit tests for business logic (VideoGenerationService)
- Integration tests for repositories
- E2E tests for critical workflows
- Manual testing via dashboard

## Troubleshooting

### Port already in use
```bash
# Find and kill process on port 5165
lsof -i :5165 | grep LISTEN | awk '{print $2}' | xargs kill
```

### Database locked
```bash
# Delete and recreate database
rm tubeorchestrator.db*
dotnet run  # Will recreate and seed
```

### Frontend can't connect to API
- Check that backend is running on `http://localhost:5165`
- Verify `.env` file in `TubeOrchestrator.Web` has correct `VITE_API_URL`
- Check browser console for CORS errors

## Contributing

This is a learning/demonstration project showcasing modern .NET and React architecture patterns for SaaS applications.

## License

MIT License - Feel free to use this as a template for your own projects!