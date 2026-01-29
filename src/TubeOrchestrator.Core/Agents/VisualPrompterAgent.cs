using Microsoft.Extensions.Logging;
using TubeOrchestrator.Core.AI;
using TubeOrchestrator.Core.Agents.Models;

namespace TubeOrchestrator.Core.Agents;

/// <summary>
/// Visual Prompter Agent: Creates image generation prompts for each script segment
/// </summary>
public class VisualPrompterAgent : BaseAgent
{
    public override string Name => "Visual Prompter";
    public override string RoleDescription => "Creates optimized image generation prompts for video segments";

    public VisualPrompterAgent(IAIProvider aiProvider, ILogger<VisualPrompterAgent> logger)
        : base(aiProvider, logger)
    {
    }

    protected override async Task ExecuteInternalAsync(JobContext context)
    {
        // Get the script from context
        var script = context.Get<string>("Script");
        if (string.IsNullOrEmpty(script))
        {
            throw new InvalidOperationException("No script available for visual prompt generation");
        }

        _logger.LogInformation("Generating visual prompts for script");

        // Split script into segments (by sections or sentences)
        var segments = SplitScriptIntoSegments(script);
        var visualPrompts = new List<VisualPrompt>();

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            
            // Create a prompt to generate an image generation prompt for this segment
            var promptGenerationPrompt = $@"Based on this video script segment, create a detailed image generation prompt for Flux/Midjourney/DALL-E.

Script segment: {segment}

Generate a concise but descriptive prompt (1-2 sentences) that:
- Captures the key visual elements
- Specifies style (photorealistic, artistic, etc.)
- Includes lighting and composition details
- Is optimized for AI image generation

Generate only the image prompt, nothing else.";

            var imagePrompt = await _aiProvider.GenerateTextAsync(
                promptGenerationPrompt, 
                temperature: 0.7, 
                maxTokens: 150);

            visualPrompts.Add(new VisualPrompt
            {
                Segment = segment,
                Prompt = imagePrompt.Trim(),
                SequenceNumber = i + 1,
                DurationSeconds = CalculateSegmentDuration(segment)
            });

            _logger.LogInformation("Visual prompt {Number}: {Prompt}", i + 1, imagePrompt.Trim());
        }

        // Store visual prompts in context
        context.Set("VisualPrompts", visualPrompts);

        _logger.LogInformation("Generated {Count} visual prompts", visualPrompts.Count);
    }

    private List<string> SplitScriptIntoSegments(string script)
    {
        // Split by common section markers or by paragraphs
        var segments = new List<string>();
        
        // Try to split by sections first (INTRO, MAIN, CONCLUSION, etc.)
        var sectionPattern = new[] { "[INTRO]", "[MAIN", "[CONCLUSION]", "[OUTRO]", "\n\n" };
        var parts = script.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed) && trimmed.Length > 50)
            {
                // If segment is too long, split by sentences
                if (trimmed.Length > 300)
                {
                    var sentences = trimmed.Split(new[] { ". ", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries);
                    var currentSegment = "";
                    
                    foreach (var sentence in sentences)
                    {
                        if (currentSegment.Length + sentence.Length < 300)
                        {
                            currentSegment += sentence + ". ";
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(currentSegment))
                            {
                                segments.Add(currentSegment.Trim());
                            }
                            currentSegment = sentence + ". ";
                        }
                    }
                    
                    if (!string.IsNullOrWhiteSpace(currentSegment))
                    {
                        segments.Add(currentSegment.Trim());
                    }
                }
                else
                {
                    segments.Add(trimmed);
                }
            }
        }

        // Ensure we have at least 3 segments and no more than 10
        if (segments.Count < 3)
        {
            segments = new List<string> { script };
        }
        else if (segments.Count > 10)
        {
            segments = segments.Take(10).ToList();
        }

        return segments;
    }

    private double CalculateSegmentDuration(string segment)
    {
        // Rough estimate: average speaking rate is ~150 words per minute
        var wordCount = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var durationMinutes = wordCount / 150.0;
        return Math.Max(3.0, durationMinutes * 60); // Minimum 3 seconds per segment
    }
}
