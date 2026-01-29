using Microsoft.AspNetCore.Mvc;
using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly ILogger<ChannelsController> _logger;
    private readonly IChannelRepository _channelRepository;

    public ChannelsController(
        ILogger<ChannelsController> logger,
        IChannelRepository channelRepository)
    {
        _logger = logger;
        _channelRepository = channelRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Channel>>> GetAll()
    {
        var channels = await _channelRepository.GetAllAsync();
        return Ok(channels);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Channel>>> GetActive()
    {
        var channels = await _channelRepository.GetActiveChannelsAsync();
        return Ok(channels);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Channel>> GetById(int id)
    {
        var channel = await _channelRepository.GetByIdAsync(id);
        if (channel == null)
        {
            return NotFound();
        }
        return Ok(channel);
    }

    [HttpPost]
    public async Task<ActionResult<Channel>> Create([FromBody] Channel channel)
    {
        var createdChannel = await _channelRepository.CreateAsync(channel);
        return CreatedAtAction(nameof(GetById), new { id = createdChannel.Id }, createdChannel);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] Channel channel)
    {
        if (id != channel.Id)
        {
            return BadRequest();
        }

        await _channelRepository.UpdateAsync(channel);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _channelRepository.DeleteAsync(id);
        return NoContent();
    }
}
