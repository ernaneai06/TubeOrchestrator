using Microsoft.AspNetCore.Mvc;
using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> _logger;
    private readonly IJobRepository _jobRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IJobQueue _jobQueue;

    public JobsController(
        ILogger<JobsController> logger,
        IJobRepository jobRepository,
        IChannelRepository channelRepository,
        IJobQueue jobQueue)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _channelRepository = channelRepository;
        _jobQueue = jobQueue;
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
}
