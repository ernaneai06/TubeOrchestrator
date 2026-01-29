using Microsoft.Extensions.Logging;
using TubeOrchestrator.Core.Agents.Models;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Infrastructure.NewsServices;

/// <summary>
/// Mock news source for testing without external API calls
/// </summary>
public class MockNewsSource : INewsSource
{
    private readonly ILogger<MockNewsSource> _logger;

    public MockNewsSource(ILogger<MockNewsSource> logger)
    {
        _logger = logger;
    }

    public Task<List<NewsItem>> FetchNewsAsync(string topic, int count = 5)
    {
        _logger.LogInformation("MockNewsSource fetching {Count} news items for topic: {Topic}", count, topic);

        var newsItems = new List<NewsItem>();
        
        for (int i = 1; i <= Math.Min(count, 5); i++)
        {
            newsItems.Add(new NewsItem
            {
                Title = $"{topic}: Breaking Development #{i} - Important Update",
                Summary = $"This is a significant development in the {topic} space. " +
                         $"Industry experts are closely monitoring the situation as it unfolds. " +
                         $"This could have major implications for the future.",
                Source = i % 2 == 0 ? "Tech News Daily" : "Industry Insider",
                Url = $"https://example.com/news/{topic.ToLower().Replace(" ", "-")}/{i}",
                PublishedAt = DateTime.UtcNow.AddHours(-i),
                Category = topic,
                Tags = new List<string> { topic, "trending", "breaking news", $"update{i}" }
            });
        }

        return Task.FromResult(newsItems);
    }
}
