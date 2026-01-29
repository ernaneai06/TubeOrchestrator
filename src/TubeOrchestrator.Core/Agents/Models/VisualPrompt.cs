namespace TubeOrchestrator.Core.Agents.Models;

/// <summary>
/// Visual prompt for image generation
/// </summary>
public class VisualPrompt
{
    public string Segment { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public double DurationSeconds { get; set; }
}
