using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using TubeOrchestrator.Core.AI;

namespace TubeOrchestrator.Core.Agents;

/// <summary>
/// Base class for all AI agents with built-in error handling and retry logic
/// </summary>
public abstract class BaseAgent
{
    protected readonly IAIProvider _aiProvider;
    protected readonly ILogger _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    /// <summary>
    /// Agent name for identification
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Description of the agent's role
    /// </summary>
    public abstract string RoleDescription { get; }

    protected BaseAgent(IAIProvider aiProvider, ILogger logger)
    {
        _aiProvider = aiProvider;
        _logger = logger;
        
        // Configure retry policy using Polly
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Agent {AgentName} retry {RetryCount} after {Delay}s due to: {Exception}",
                        Name, retryCount, timeSpan.TotalSeconds, exception.Message);
                });
    }

    /// <summary>
    /// Executes the agent's task within the job context
    /// </summary>
    public async Task ExecuteAsync(JobContext context)
    {
        _logger.LogInformation("Agent {AgentName} starting execution for Job {JobId}", 
            Name, context.Job.Id);

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await ExecuteInternalAsync(context);
            });

            _logger.LogInformation("Agent {AgentName} completed execution for Job {JobId}", 
                Name, context.Job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentName} failed for Job {JobId}: {Error}", 
                Name, context.Job.Id, ex.Message);
            throw new AgentExecutionException(Name, ex.Message, ex);
        }
    }

    /// <summary>
    /// Internal implementation of agent logic - to be implemented by derived classes
    /// </summary>
    protected abstract Task ExecuteInternalAsync(JobContext context);
}

/// <summary>
/// Exception thrown when an agent fails to execute
/// </summary>
public class AgentExecutionException : Exception
{
    public string AgentName { get; }

    public AgentExecutionException(string agentName, string message, Exception? innerException = null)
        : base($"Agent {agentName} failed: {message}", innerException)
    {
        AgentName = agentName;
    }
}
