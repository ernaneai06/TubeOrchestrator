using Microsoft.Extensions.Logging;
using TubeOrchestrator.Core.AI;

namespace TubeOrchestrator.Infrastructure.AI;

/// <summary>
/// Mock AI provider for testing and development without consuming API tokens
/// </summary>
public class MockAIProvider : IAIProvider
{
    private readonly ILogger<MockAIProvider> _logger;

    public string ProviderName => "Mock";

    public MockAIProvider(ILogger<MockAIProvider> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateTextAsync(string prompt, double temperature = 0.7, int maxTokens = 2000)
    {
        _logger.LogInformation("MockAI generating text (temperature: {Temperature})", temperature);
        
        // Return deterministic mock responses based on prompt content
        if (prompt.Contains("script", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(@"[INTRO]
Welcome to our channel! Today we're diving into an exciting topic.

[MAIN CONTENT]
Here's what you need to know: This is a comprehensive look at the subject matter. 
We'll explore the key points and provide valuable insights that you won't want to miss.

The story unfolds with interesting developments that keep viewers engaged.
Expert analysis shows that this topic has significant implications.

[CONCLUSION]
Thank you for watching! Don't forget to like and subscribe for more content.
Hit that notification bell to stay updated!");
        }
        
        if (prompt.Contains("title", StringComparison.OrdinalIgnoreCase) || 
            prompt.Contains("seo", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult("🚀 Amazing Discovery: What You Need to Know Right Now! 🔥");
        }
        
        if (prompt.Contains("description", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(@"In this video, we explore an important topic that matters to you.

Key Points:
• Comprehensive analysis
• Expert insights
• Actionable takeaways

🔔 Subscribe for more content
👍 Like if you found this helpful
💬 Comment your thoughts below

#trending #viral #educational");
        }
        
        if (prompt.Contains("tags", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult("trending, viral, educational, news, technology, informative, must watch, 2024, latest update");
        }
        
        if (prompt.Contains("image", StringComparison.OrdinalIgnoreCase) || 
            prompt.Contains("visual", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult("A vibrant, eye-catching scene with dynamic composition, professional lighting, and engaging visual elements that capture the essence of the content");
        }

        // Default response
        return Task.FromResult($"Mock AI response to prompt (length: {prompt.Length} chars). This is simulated content for testing purposes without consuming API tokens.");
    }

    public Task<string> AnalyzeImageAsync(string imageUrl, string? prompt = null)
    {
        _logger.LogInformation("MockAI analyzing image: {ImageUrl}", imageUrl);
        
        return Task.FromResult(@"Mock image analysis: The image shows a well-composed scene with good lighting and clear subject matter. 
The visual elements are arranged in an aesthetically pleasing manner. 
Key features include balanced composition, appropriate color palette, and effective use of space.
This would work well for thumbnail or promotional purposes.");
    }
}
