namespace TubeOrchestrator.Core.Entities;

public class Channel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Platform { get; set; } = "YouTube"; // YouTube/TikTok
    public string? CredentialsJson { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ScheduleCron { get; set; }
    public int? NicheId { get; set; }
    public Niche? Niche { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
