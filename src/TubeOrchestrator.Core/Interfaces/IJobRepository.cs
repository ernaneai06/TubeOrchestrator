using TubeOrchestrator.Core.Entities;

namespace TubeOrchestrator.Core.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(int id);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<IEnumerable<Job>> GetByChannelIdAsync(int channelId);
    Task<IEnumerable<Job>> GetRecentJobsAsync(int count = 10);
    Task<Job> CreateAsync(Job job);
    Task UpdateAsync(Job job);
    Task DeleteAsync(int id);
}
