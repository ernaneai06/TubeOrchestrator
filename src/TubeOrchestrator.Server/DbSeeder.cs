using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Data;

namespace TubeOrchestrator.Server;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if we already have data
        if (context.Niches.Any())
        {
            return; // Database already seeded
        }

        // Create niches
        var techNiche = new Niche
        {
            Name = "Tech News",
            Description = "Technology and innovation news"
        };

        var meditationNiche = new Niche
        {
            Name = "Meditation",
            Description = "Mindfulness and meditation content"
        };

        context.Niches.AddRange(techNiche, meditationNiche);
        await context.SaveChangesAsync();

        // Create prompt templates
        var promptTemplates = new[]
        {
            new PromptTemplate
            {
                NicheId = techNiche.Id,
                Type = "Script",
                TemplateText = "Create a video script about {{NEWS_DATA}} for a tech-savvy audience. Make it engaging and informative."
            },
            new PromptTemplate
            {
                NicheId = techNiche.Id,
                Type = "Title",
                TemplateText = "Generate a catchy YouTube title for: {{NEWS_DATA}}"
            },
            new PromptTemplate
            {
                NicheId = techNiche.Id,
                Type = "Description",
                TemplateText = "Write a YouTube description for a video about: {{NEWS_DATA}}"
            },
            new PromptTemplate
            {
                NicheId = meditationNiche.Id,
                Type = "Script",
                TemplateText = "Create a calming meditation script focused on {{TOPIC}}. Duration: 10 minutes."
            }
        };

        context.PromptTemplates.AddRange(promptTemplates);
        await context.SaveChangesAsync();

        // Create sample channels
        var channels = new[]
        {
            new Channel
            {
                Name = "Tech Daily",
                Platform = "YouTube",
                NicheId = techNiche.Id,
                IsActive = true,
                ScheduleCron = "0 9 * * *" // Daily at 9 AM
            },
            new Channel
            {
                Name = "Mindful Moments",
                Platform = "YouTube",
                NicheId = meditationNiche.Id,
                IsActive = true,
                ScheduleCron = "0 18 * * *" // Daily at 6 PM
            },
            new Channel
            {
                Name = "Tech Shorts",
                Platform = "TikTok",
                NicheId = techNiche.Id,
                IsActive = false
            }
        };

        context.Channels.AddRange(channels);
        await context.SaveChangesAsync();
    }
}
