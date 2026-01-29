namespace TubeOrchestrator.Core.Entities;

public class Job
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public Channel? Channel { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Processing_ParallelActions, WaitingForApproval, Completed, Failed
    public string? CurrentAgent { get; set; } // Name of currently executing agent
    public int StepProgress { get; set; } = 0; // Progress 0-100
    public string? Script { get; set; } // Generated script (for approval workflow)
    public string? LogOutput { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
