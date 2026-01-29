using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Core.Services;

public class JobQueue : IJobQueue
{
    private readonly System.Threading.Channels.Channel<Job> _channel;

    public JobQueue(int capacity = 100)
    {
        var options = new System.Threading.Channels.BoundedChannelOptions(capacity)
        {
            FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait
        };
        _channel = System.Threading.Channels.Channel.CreateBounded<Job>(options);
    }

    public async ValueTask EnqueueAsync(Job job)
    {
        await _channel.Writer.WriteAsync(job);
    }

    public async ValueTask<Job> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
