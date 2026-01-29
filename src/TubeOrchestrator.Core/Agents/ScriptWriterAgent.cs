using Microsoft.Extensions.Logging;
using System.Text;
using TubeOrchestrator.Core.AI;
using TubeOrchestrator.Core.Agents.Models;

namespace TubeOrchestrator.Core.Agents;

/// <summary>
/// Script Writer Agent: Generates video scripts based on news items and prompt templates
/// </summary>
public class ScriptWriterAgent : BaseAgent
{
    public override string Name => "Script Writer";
    public override string RoleDescription => "Creates engaging video scripts formatted for text-to-speech";

    public ScriptWriterAgent(IAIProvider aiProvider, ILogger<ScriptWriterAgent> logger)
        : base(aiProvider, logger)
    {
    }

    protected override async Task ExecuteInternalAsync(JobContext context)
    {
        // Get news items from context
        var newsItems = context.Get<List<NewsItem>>("NewsItems");
        if (newsItems == null || newsItems.Count == 0)
        {
            throw new InvalidOperationException("No news items available for script generation");
        }

        var channel = context.Channel;
        var niche = channel.Niche;

        _logger.LogInformation("Generating script for channel {Channel} with {Count} news items", 
            channel.Name, newsItems.Count);

        // Build the news data content
        var newsData = new StringBuilder();
        for (int i = 0; i < newsItems.Count; i++)
        {
            newsData.AppendLine($"{i + 1}. {newsItems[i].Title}");
            newsData.AppendLine($"   {newsItems[i].Summary}");
            newsData.AppendLine();
        }

        // Get the prompt template from the niche
        var scriptTemplate = niche?.PromptTemplates?.FirstOrDefault(pt => pt.Type == "Script");
        string prompt;

        if (scriptTemplate != null)
        {
            // Use the configured template
            prompt = scriptTemplate.TemplateText
                .Replace("{{NEWS_DATA}}", newsData.ToString())
                .Replace("{{TOPIC}}", niche?.Name ?? "")
                .Replace("{{CHANNEL_NAME}}", channel.Name)
                .Replace("{{TONE}}", "professional and engaging"); // Could be a channel config
        }
        else
        {
            // Fallback template
            prompt = $@"Create an engaging video script for a YouTube video about {niche?.Name ?? "news"}.

Channel: {channel.Name}
Tone: Professional and engaging, suitable for text-to-speech narration

News Items:
{newsData}

Generate a complete video script with:
- INTRO: Hook the viewer in the first 10 seconds
- MAIN CONTENT: Cover each news item engagingly
- CONCLUSION: Call to action (like, subscribe, comment)

Format the script clearly for TTS, avoiding special characters and using natural speech patterns.";
        }

        // Generate the script using AI
        var script = await _aiProvider.GenerateTextAsync(prompt, temperature: 0.7, maxTokens: 3000);

        // Store the script in the context
        context.Set("Script", script);
        
        _logger.LogInformation("Script generated: {Length} characters", script.Length);
    }
}
