using Microsoft.AspNetCore.Mvc;
using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Core.Interfaces;
using TubeOrchestrator.Worker.Services;

namespace TubeOrchestrator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> _logger;
    private readonly IJobRepository _jobRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IJobQueue _jobQueue;
    private readonly IServiceProvider _serviceProvider;

    public JobsController(
        ILogger<JobsController> logger,
        IJobRepository jobRepository,
        IChannelRepository channelRepository,
        IJobQueue jobQueue,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _channelRepository = channelRepository;
        _jobQueue = jobQueue;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Job>>> GetAll()
    {
        var jobs = await _jobRepository.GetAllAsync();
        return Ok(jobs);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<Job>>> GetRecent([FromQuery] int count = 10)
    {
        var jobs = await _jobRepository.GetRecentJobsAsync(count);
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Job>> GetById(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null)
        {
            return NotFound();
        }
        return Ok(job);
    }

    [HttpPost("trigger/{channelId}")]
    public async Task<ActionResult<Job>> TriggerJob(int channelId)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId);
        if (channel == null)
        {
            return NotFound($"Channel {channelId} not found");
        }

        if (!channel.IsActive)
        {
            return BadRequest($"Channel {channelId} is not active");
        }

        var job = new Job
        {
            ChannelId = channelId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var createdJob = await _jobRepository.CreateAsync(job);
        
        // Enqueue the job for processing
        await _jobQueue.EnqueueAsync(createdJob);
        
        _logger.LogInformation("Job {JobId} created and enqueued for Channel {ChannelId}", createdJob.Id, channelId);

        return CreatedAtAction(nameof(GetById), new { id = createdJob.Id }, createdJob);
    }

    /// <summary>
    /// Approve and optionally edit a script, then continue job processing
    /// Phase 10: Human-in-the-Loop approval workflow
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult> ApproveJob(int id, [FromBody] ApproveJobRequest request)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null)
        {
            return NotFound($"Job {id} not found");
        }

        if (job.Status != "WaitingForApproval")
        {
            return BadRequest($"Job {id} is not waiting for approval (current status: {job.Status})");
        }

        var channel = await _channelRepository.GetByIdAsync(job.ChannelId);
        if (channel == null)
        {
            return NotFound($"Channel {job.ChannelId} not found");
        }

        _logger.LogInformation("Job {JobId} approved, continuing processing", id);

        // Update job with approved script
        job.Script = request.ApprovedScript ?? job.Script;
        job.Status = "Processing";
        await _jobRepository.UpdateAsync(job);

        // Continue processing in background
        _ = Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var videoService = scope.ServiceProvider.GetRequiredService<VideoGenerationService>();
            var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            
            try
            {
                var videoUrl = await videoService.ContinueAfterApprovalAsync(job, channel, request.ApprovedScript ?? job.Script ?? "");
                
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;
                job.VideoUrl = videoUrl;
                job.LogOutput = $"Video successfully generated after approval. URL: {videoUrl}";
                
                await jobRepo.UpdateAsync(job);
                _logger.LogInformation("Job {JobId} completed after approval", id);
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.CompletedAt = DateTime.UtcNow;
                job.LogOutput = $"Error after approval: {ex.Message}";
                
                await jobRepo.UpdateAsync(job);
                _logger.LogError(ex, "Job {JobId} failed after approval", id);
            }
        });

        return Ok(new { message = "Job approved and processing resumed", jobId = id });
    }
}

public class ApproveJobRequest
{
    public string? ApprovedScript { get; set; }
}
