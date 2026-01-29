namespace TubeOrchestrator.Core.Entities;

public class Niche
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    public ICollection<PromptTemplate> PromptTemplates { get; set; } = new List<PromptTemplate>();
}
