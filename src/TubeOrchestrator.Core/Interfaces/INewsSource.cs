using TubeOrchestrator.Core.Agents.Models;

namespace TubeOrchestrator.Core.Interfaces;

/// <summary>
/// Interface for news/content sources (RSS, Grok, NewsAPI, etc.)
/// </summary>
public interface INewsSource
{
    /// <summary>
    /// Fetches top news items for a specific topic/niche
    /// </summary>
    /// <param name="topic">The topic or niche to search for</param>
    /// <param name="count">Number of items to return (default: 5)</param>
    /// <returns>List of news items</returns>
    Task<List<NewsItem>> FetchNewsAsync(string topic, int count = 5);
}
