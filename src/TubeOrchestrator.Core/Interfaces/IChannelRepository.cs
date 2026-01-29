using TubeOrchestrator.Core.Entities;

namespace TubeOrchestrator.Core.Interfaces;

public interface IChannelRepository
{
    Task<Channel?> GetByIdAsync(int id);
    Task<IEnumerable<Channel>> GetAllAsync();
    Task<IEnumerable<Channel>> GetActiveChannelsAsync();
    Task<Channel> CreateAsync(Channel channel);
    Task UpdateAsync(Channel channel);
    Task DeleteAsync(int id);
}
