using Microsoft.Extensions.Logging;
using TubeOrchestrator.Core.AI;
using TubeOrchestrator.Core.Agents.Models;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Core.Agents;

/// <summary>
/// Research Agent: Finds top news items based on channel niche
/// </summary>
public class ResearchAgent : BaseAgent
{
    private readonly INewsSource _newsSource;

    public override string Name => "Research Agent";
    public override string RoleDescription => "Discovers and curates top news items based on channel niche";

    public ResearchAgent(IAIProvider aiProvider, ILogger<ResearchAgent> logger, INewsSource newsSource)
        : base(aiProvider, logger)
    {
        _newsSource = newsSource;
    }

    protected override async Task ExecuteInternalAsync(JobContext context)
    {
        var niche = context.Channel.Niche?.Name ?? "General";
        _logger.LogInformation("Researching news for niche: {Niche}", niche);

        // Fetch news items from the news source
        var newsItems = await _newsSource.FetchNewsAsync(niche, count: 5);
        
        if (newsItems.Count == 0)
        {
            throw new InvalidOperationException($"No news items found for niche: {niche}");
        }

        // Optionally use AI to enrich/summarize the news
        foreach (var item in newsItems)
        {
            if (string.IsNullOrEmpty(item.Summary) && !string.IsNullOrEmpty(item.Title))
            {
                var enrichmentPrompt = $"Create a brief, engaging summary (2-3 sentences) for this news headline: {item.Title}";
                item.Summary = await _aiProvider.GenerateTextAsync(enrichmentPrompt, temperature: 0.5, maxTokens: 150);
            }
        }

        // Store the news items in the context for other agents to use
        context.Set("NewsItems", newsItems);
        
        _logger.LogInformation("Research complete: Found {Count} news items", newsItems.Count);
    }
}
