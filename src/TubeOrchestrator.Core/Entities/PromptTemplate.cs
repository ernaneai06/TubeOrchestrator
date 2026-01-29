namespace TubeOrchestrator.Core.Entities;

public class PromptTemplate
{
    public int Id { get; set; }
    public int NicheId { get; set; }
    public Niche? Niche { get; set; }
    public string Type { get; set; } = string.Empty; // Script/Title/Description
    public string TemplateText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
