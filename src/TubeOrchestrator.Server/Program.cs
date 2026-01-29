using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
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
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/tubeorchestrator-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting TubeOrchestrator API Server");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    builder.Services.AddOpenApi();

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>();

    // Configure CORS for React frontend
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

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
        var apiKey = builder.Configuration["DeepSeek:ApiKey"] ?? "your-api-key-here";
        builder.Services.AddHttpClient<IAIProvider, DeepSeekProvider>()
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(60));
        builder.Services.AddScoped<IAIProvider>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            var logger = sp.GetRequiredService<ILogger<DeepSeekProvider>>();
            return new DeepSeekProvider(httpClient, logger, apiKey);
        });
        Log.Information("Using DeepSeekProvider for AI generation");
    }

    // Register News Source
    builder.Services.AddScoped<INewsSource, MockNewsSource>();

    // Register AI Agents
    builder.Services.AddScoped<ResearchAgent>();
    builder.Services.AddScoped<ScriptWriterAgent>();
    builder.Services.AddScoped<SeoSpecialistAgent>();
    builder.Services.AddScoped<VisualPrompterAgent>();

    // Register services
    builder.Services.AddScoped<VideoGenerationService>();

    // Register JobQueue as singleton (shared between API and Worker)
    builder.Services.AddSingleton<IJobQueue, JobQueue>();

    // Register the Worker as a hosted service
    builder.Services.AddHostedService<VideoProcessingWorker>();

    var app = builder.Build();

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await TubeOrchestrator.Server.DbSeeder.SeedAsync(context);
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Add Serilog request logging
    app.UseSerilogRequestLogging();

    // Map health check endpoint
    app.MapHealthChecks("/health");

    app.UseCors("AllowReactApp");
    app.UseHttpsRedirection();
    app.MapControllers();

    Log.Information("TubeOrchestrator API Server started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
