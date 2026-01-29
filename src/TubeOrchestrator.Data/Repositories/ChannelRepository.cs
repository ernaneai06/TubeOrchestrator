using Microsoft.EntityFrameworkCore;
using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Data.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly AppDbContext _context;

    public ChannelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Channel?> GetByIdAsync(int id)
    {
        return await _context.Channels
            .Include(c => c.Niche)
                .ThenInclude(n => n!.PromptTemplates)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Channel>> GetAllAsync()
    {
        return await _context.Channels
            .Include(c => c.Niche)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Channel>> GetActiveChannelsAsync()
    {
        return await _context.Channels
            .Include(c => c.Niche)
                .ThenInclude(n => n!.PromptTemplates)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Channel> CreateAsync(Channel channel)
    {
        _context.Channels.Add(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task UpdateAsync(Channel channel)
    {
        channel.UpdatedAt = DateTime.UtcNow;
        _context.Channels.Update(channel);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var channel = await _context.Channels.FindAsync(id);
        if (channel != null)
        {
            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync();
        }
    }
}
