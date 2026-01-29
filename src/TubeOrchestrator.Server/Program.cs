using Microsoft.EntityFrameworkCore;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddOpenApi();

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

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
