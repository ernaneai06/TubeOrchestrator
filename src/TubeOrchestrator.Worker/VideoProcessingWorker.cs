using Microsoft.Extensions.DependencyInjection;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Worker;

public class VideoProcessingWorker : BackgroundService
{
    private readonly ILogger<VideoProcessingWorker> _logger;
    private readonly IJobQueue _jobQueue;
    private readonly IServiceProvider _serviceProvider;

    public VideoProcessingWorker(
        ILogger<VideoProcessingWorker> logger,
        IJobQueue jobQueue,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _jobQueue = jobQueue;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VideoProcessingWorker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _jobQueue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Processing Job {JobId} for Channel {ChannelId}", job.Id, job.ChannelId);

                // Create a scope for database operations to avoid DbContext conflicts
                using var scope = _serviceProvider.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                // Update job status to Processing
                job.Status = "Processing";
                job.StartedAt = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job);

                try
                {
                    // Simulate video processing (replace with actual VideoGenerationService later)
                    await Task.Delay(5000, stoppingToken); // Simulate 5 seconds of work
                    
                    job.Status = "Completed";
                    job.CompletedAt = DateTime.UtcNow;
                    job.LogOutput = "Video processing completed successfully (simulated)";
                    job.VideoUrl = $"https://example.com/videos/{job.Id}";
                    
                    _logger.LogInformation("Job {JobId} completed successfully", job.Id);
                }
                catch (Exception ex)
                {
                    job.Status = "Failed";
                    job.CompletedAt = DateTime.UtcNow;
                    job.LogOutput = $"Error: {ex.Message}";
                    _logger.LogError(ex, "Job {JobId} failed", job.Id);
                }

                await jobRepository.UpdateAsync(job);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("VideoProcessingWorker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job");
                await Task.Delay(1000, stoppingToken); // Wait before retrying
            }
        }
    }
}
