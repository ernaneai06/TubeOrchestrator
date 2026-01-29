using Microsoft.Extensions.Logging;
using TubeOrchestrator.Core.AI;
using TubeOrchestrator.Core.Agents.Models;

namespace TubeOrchestrator.Core.Agents;

/// <summary>
/// SEO Specialist Agent: Generates title, description, tags, and thumbnail suggestions
/// </summary>
public class SeoSpecialistAgent : BaseAgent
{
    public override string Name => "SEO Specialist";
    public override string RoleDescription => "Creates optimized titles, descriptions, tags, and thumbnail suggestions";

    public SeoSpecialistAgent(IAIProvider aiProvider, ILogger<SeoSpecialistAgent> logger)
        : base(aiProvider, logger)
    {
    }

    protected override async Task ExecuteInternalAsync(JobContext context)
    {
        // Get the script from context
        var script = context.Get<string>("Script");
        if (string.IsNullOrEmpty(script))
        {
            throw new InvalidOperationException("No script available for SEO generation");
        }

        var channel = context.Channel;
        var niche = channel.Niche?.Name ?? "General";

        _logger.LogInformation("Generating SEO metadata for {Niche} content", niche);

        // Create prompts for each SEO element
        var scriptExcerpt = script.Length > 500 ? script.Substring(0, 500) : script;
        var scriptExcerptShort = script.Length > 300 ? script.Substring(0, 300) : script;

        var titlePrompt = $@"Based on this video script, create a compelling YouTube title that is:
- Attention-grabbing but honest (no misleading clickbait)
- 60 characters or less
- Includes relevant keywords
- Uses emojis strategically

Script excerpt: {scriptExcerpt}

Generate only the title, nothing else.";

        var descriptionPrompt = $@"Based on this video script, create a YouTube video description that:
- Summarizes the video content (3-4 sentences)
- Includes a call-to-action
- Uses relevant keywords naturally
- Includes relevant hashtags at the end

Script excerpt: {scriptExcerpt}

Generate only the description.";

        var tagsPrompt = $@"Based on this video about {niche}, generate 8-12 relevant YouTube tags.
Tags should be comma-separated, include both broad and specific terms.

Script excerpt: {scriptExcerptShort}

Generate only the tags as a comma-separated list.";

        var thumbnailPrompt = $@"Based on this video script, suggest a compelling thumbnail concept.
Describe the visual elements, text overlay, and overall composition in 2-3 sentences.

Script excerpt: {scriptExcerptShort}

Generate only the thumbnail description.";

        // Generate all SEO elements (can be done in parallel, but keeping sequential for simplicity)
        var title = await _aiProvider.GenerateTextAsync(titlePrompt, temperature: 0.8, maxTokens: 100);
        var description = await _aiProvider.GenerateTextAsync(descriptionPrompt, temperature: 0.7, maxTokens: 300);
        var tagsText = await _aiProvider.GenerateTextAsync(tagsPrompt, temperature: 0.6, maxTokens: 150);
        var thumbnailSuggestion = await _aiProvider.GenerateTextAsync(thumbnailPrompt, temperature: 0.7, maxTokens: 200);

        // Parse tags from comma-separated text
        var tags = tagsText
            .Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        var seoMetadata = new SeoMetadata
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Tags = tags,
            ThumbnailSuggestion = thumbnailSuggestion.Trim()
        };

        // Store SEO metadata in context
        context.Set("SeoMetadata", seoMetadata);

        _logger.LogInformation("SEO metadata generated: Title='{Title}', Tags={TagCount}", 
            seoMetadata.Title, seoMetadata.Tags.Count);
    }
}
