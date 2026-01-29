using Microsoft.Extensions.DependencyInjection;
using TubeOrchestrator.Core.Interfaces;
using TubeOrchestrator.Worker.Services;

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
                var channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();
                var videoService = scope.ServiceProvider.GetRequiredService<VideoGenerationService>();

                // Get the full channel with related data
                var channel = await channelRepository.GetByIdAsync(job.ChannelId);
                if (channel == null)
                {
                    _logger.LogError("Channel {ChannelId} not found for Job {JobId}", job.ChannelId, job.Id);
                    job.Status = "Failed";
                    job.CompletedAt = DateTime.UtcNow;
                    job.LogOutput = $"Channel {job.ChannelId} not found";
                    await jobRepository.UpdateAsync(job);
                    continue;
                }

                // Update job status to Processing
                job.Status = "Processing";
                job.StartedAt = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job);

                try
                {
                    // Generate the video using the VideoGenerationService
                    var videoUrl = await videoService.GenerateVideoAsync(job, channel);
                    
                    job.Status = "Completed";
                    job.CompletedAt = DateTime.UtcNow;
                    job.LogOutput = $"Video successfully generated and uploaded. Channel: {channel.Name}, Niche: {channel.Niche?.Name ?? "N/A"}";
                    job.VideoUrl = videoUrl;
                    
                    _logger.LogInformation("Job {JobId} completed successfully with video URL: {VideoUrl}", job.Id, videoUrl);
                }
                catch (Exception ex)
                {
                    job.Status = "Failed";
                    job.CompletedAt = DateTime.UtcNow;
                    job.LogOutput = $"Error: {ex.Message}\n{ex.StackTrace}";
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
