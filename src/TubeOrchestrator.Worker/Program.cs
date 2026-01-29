using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting TubeOrchestrator Worker");

    var builder = Host.CreateApplicationBuilder(args);

    // Use Serilog for logging
    builder.Services.AddSerilog();

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
        Log.Information("Using MockAIProvider for testing (set UseMockAI=false for real AI)");
    }
    else
    {
        // Use real AI provider (DeepSeek by default)
        var apiKey = builder.Configuration["DeepSeek:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your-api-key-here")
        {
            throw new InvalidOperationException("DeepSeek API key is required when UseMockAI=false. Please set DeepSeek:ApiKey in configuration.");
        }
        
        builder.Services.AddHttpClient<DeepSeekProvider>()
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(60));
        
        builder.Services.AddScoped<IAIProvider, DeepSeekProvider>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(DeepSeekProvider));
            var logger = sp.GetRequiredService<ILogger<DeepSeekProvider>>();
            return new DeepSeekProvider(httpClient, logger, apiKey);
        });
        Log.Information("Using DeepSeekProvider for AI generation");
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

    Log.Information("TubeOrchestrator Worker started successfully");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
