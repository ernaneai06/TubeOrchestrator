using Microsoft.EntityFrameworkCore;
using TubeOrchestrator.Core.AI;
using TubeOrchestrator.Core.Agents;
using TubeOrchestrator.Core.Interfaces;
using TubeOrchestrator.Core.Services;
using TubeOrchestrator.Data;
using TubeOrchestrator.Data.Repositories;
using TubeOrchestrator.Infrastructure.AI;
using TubeOrchestrator.Infrastructure.NewsServices;
using TubeOrchestrator.Worker;
using TubeOrchestrator.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=tubeorchestrator.db"));

// Register repositories
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();

// Register AI Provider (check config for mock mode)
var useMockAI = builder.Configuration.GetValue<bool>("UseMockAI", true);
if (useMockAI)
{
    builder.Services.AddScoped<IAIProvider, MockAIProvider>();
    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
    Console.WriteLine("⚠️  Using MockAIProvider for testing (set UseMockAI=false for real AI)");
}
else
{
    // Use real AI provider (DeepSeek by default)
    var apiKey = builder.Configuration["DeepSeek:ApiKey"] ?? "your-api-key-here";
    builder.Services.AddHttpClient<IAIProvider, DeepSeekProvider>()
        .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(60));
    builder.Services.AddScoped<IAIProvider>(sp =>
    {
        var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
        var logger = sp.GetRequiredService<ILogger<DeepSeekProvider>>();
        return new DeepSeekProvider(httpClient, logger, apiKey);
    });
}

// Register News Source (mock for now)
builder.Services.AddScoped<INewsSource, MockNewsSource>();

// Register AI Agents as Scoped
builder.Services.AddScoped<ResearchAgent>();
builder.Services.AddScoped<ScriptWriterAgent>();
builder.Services.AddScoped<SeoSpecialistAgent>();
builder.Services.AddScoped<VisualPrompterAgent>();

// Register services
builder.Services.AddScoped<VideoGenerationService>();

// Register JobQueue as singleton
builder.Services.AddSingleton<IJobQueue, JobQueue>();

// Register Worker
builder.Services.AddHostedService<VideoProcessingWorker>();

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
}

host.Run();
