using Microsoft.EntityFrameworkCore;
using TubeOrchestrator.Core.Interfaces;
using TubeOrchestrator.Core.Services;
using TubeOrchestrator.Data;
using TubeOrchestrator.Data.Repositories;
using TubeOrchestrator.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=tubeorchestrator.db"));

// Register repositories
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();

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
