using TubeOrchestrator.Core.Entities;

namespace TubeOrchestrator.Core.Interfaces;

public interface IJobQueue
{
    ValueTask EnqueueAsync(Job job);
    ValueTask<Job> DequeueAsync(CancellationToken cancellationToken);
}
