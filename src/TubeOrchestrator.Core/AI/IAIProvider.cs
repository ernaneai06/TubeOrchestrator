namespace TubeOrchestrator.Core.AI;

/// <summary>
/// Generic interface for AI providers that can generate text and analyze content
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Generates text based on a prompt
    /// </summary>
    /// <param name="prompt">The input prompt for text generation</param>
    /// <param name="temperature">Controls randomness (0.0 = deterministic, 1.0 = creative)</param>
    /// <param name="maxTokens">Maximum number of tokens to generate</param>
    /// <returns>Generated text</returns>
    Task<string> GenerateTextAsync(string prompt, double temperature = 0.7, int maxTokens = 2000);

    /// <summary>
    /// Analyzes an image from a URL
    /// </summary>
    /// <param name="imageUrl">URL of the image to analyze</param>
    /// <param name="prompt">Optional prompt for specific analysis</param>
    /// <returns>Analysis result as text</returns>
    Task<string> AnalyzeImageAsync(string imageUrl, string? prompt = null);

    /// <summary>
    /// Gets the provider name
    /// </summary>
    string ProviderName { get; }
}
