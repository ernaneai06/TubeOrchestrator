using TubeOrchestrator.Core.Agents;
using TubeOrchestrator.Core.Agents.Models;
using TubeOrchestrator.Core.Entities;
using TubeOrchestrator.Core.Interfaces;

namespace TubeOrchestrator.Worker.Services;

/// <summary>
/// Orchestrates the video generation workflow using specialized AI agents
/// with parallel processing for optimal performance
/// </summary>
public class VideoGenerationService
{
    private readonly ILogger<VideoGenerationService> _logger;
    private readonly ResearchAgent _researchAgent;
    private readonly ScriptWriterAgent _scriptWriterAgent;
    private readonly SeoSpecialistAgent _seoAgent;
    private readonly VisualPrompterAgent _visualAgent;
    private readonly IJobRepository _jobRepository;

    public VideoGenerationService(
        ILogger<VideoGenerationService> logger,
        ResearchAgent researchAgent,
        ScriptWriterAgent scriptWriterAgent,
        SeoSpecialistAgent seoAgent,
        VisualPrompterAgent visualAgent,
        IJobRepository jobRepository)
    {
        _logger = logger;
        _researchAgent = researchAgent;
        _scriptWriterAgent = scriptWriterAgent;
        _seoAgent = seoAgent;
        _visualAgent = visualAgent;
        _jobRepository = jobRepository;
    }

    public async Task<string> GenerateVideoAsync(Job job, Channel channel)
    {
        _logger.LogInformation("Starting agent-orchestrated video generation for Job {JobId}", job.Id);

        var context = new JobContext
        {
            Job = job,
            Channel = channel
        };

        try
        {
            // **STEP 1: Research (Sequential - we need the content base)**
            await UpdateJobProgress(job, "Research Agent", 10);
            await _researchAgent.ExecuteAsync(context);
            _logger.LogInformation("Research complete");

            // **STEP 2: Script Writing (Sequential - we need the script for next steps)**
            await UpdateJobProgress(job, "Script Writer", 30);
            await _scriptWriterAgent.ExecuteAsync(context);
            var script = context.Get<string>("Script") ?? throw new InvalidOperationException("Script not generated");
            _logger.LogInformation("Script complete: {Length} chars", script.Length);

            // Save script to job for potential approval workflow
            job.Script = script;
            await _jobRepository.UpdateAsync(job);

            // Check if approval is required
            if (channel.RequireApproval)
            {
                _logger.LogInformation("Job {JobId} requires approval, pausing workflow", job.Id);
                job.Status = "WaitingForApproval";
                job.CurrentAgent = "Awaiting Human Approval";
                job.StepProgress = 40;
                await _jobRepository.UpdateAsync(job);
                
                // Return empty string to indicate workflow is paused
                return string.Empty;
            }

            // **STEP 3: PARALLEL ORCHESTRATION (The Magic!)**
            await UpdateJobProgress(job, "Parallel Processing", 50);
            job.Status = "Processing_ParallelActions";
            await _jobRepository.UpdateAsync(job);

            _logger.LogInformation("Starting parallel execution of SEO, Visual, and Audio generation");

            // Execute SEO, Visual Prompts, and Audio in parallel
            // Note: These are already async methods, so they execute concurrently when awaited together
            await Task.WhenAll(
                _seoAgent.ExecuteAsync(context),
                _visualAgent.ExecuteAsync(context),
                GenerateAudioAsync(script)
            );
            
            _logger.LogInformation("All parallel tasks completed successfully");

            await UpdateJobProgress(job, "Rendering Video", 80);

            // **STEP 4: Video Assembly (Sequential - needs outputs from parallel tasks)**
            var seoMetadata = context.Get<SeoMetadata>("SeoMetadata");
            var visualPrompts = context.Get<List<VisualPrompt>>("VisualPrompts");
            
            var videoUrl = await AssembleAndUploadVideoAsync(script, seoMetadata, visualPrompts, channel);

            await UpdateJobProgress(job, "Completed", 100);
            
            _logger.LogInformation("Video generation complete: {VideoUrl}", videoUrl);
            return videoUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in agent orchestration for Job {JobId}", job.Id);
            job.CurrentAgent = "Failed";
            job.StepProgress = 0;
            await _jobRepository.UpdateAsync(job);
            throw;
        }
    }

    /// <summary>
    /// Continues job execution after approval (called from approval endpoint)
    /// </summary>
    public async Task<string> ContinueAfterApprovalAsync(Job job, Channel channel, string approvedScript)
    {
        _logger.LogInformation("Continuing Job {JobId} after approval", job.Id);

        var context = new JobContext
        {
            Job = job,
            Channel = channel
        };

        // Set the approved script in context
        context.Set("Script", approvedScript);
        job.Script = approvedScript;
        job.Status = "Processing";
        await _jobRepository.UpdateAsync(job);

        // Continue from parallel orchestration step
        await UpdateJobProgress(job, "Parallel Processing", 50);
        job.Status = "Processing_ParallelActions";
        await _jobRepository.UpdateAsync(job);

        // Execute parallel tasks - already async methods execute concurrently
        await Task.WhenAll(
            _seoAgent.ExecuteAsync(context),
            _visualAgent.ExecuteAsync(context),
            GenerateAudioAsync(approvedScript)
        );

        await UpdateJobProgress(job, "Rendering Video", 80);

        var seoMetadata = context.Get<SeoMetadata>("SeoMetadata");
        var visualPrompts = context.Get<List<VisualPrompt>>("VisualPrompts");
        
        var videoUrl = await AssembleAndUploadVideoAsync(approvedScript, seoMetadata, visualPrompts, channel);

        await UpdateJobProgress(job, "Completed", 100);
        
        return videoUrl;
    }

    private async Task UpdateJobProgress(Job job, string currentAgent, int progress)
    {
        job.CurrentAgent = currentAgent;
        job.StepProgress = progress;
        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job {JobId} progress: {Agent} - {Progress}%", job.Id, currentAgent, progress);
    }

    private async Task GenerateAudioAsync(string script)
    {
        // Simulate TTS audio generation
        _logger.LogInformation("Generating audio from script ({Length} chars)", script.Length);
        await Task.Delay(2000); // Simulate IO-bound audio generation
        
        // In real implementation:
        // - Call a TTS service (ElevenLabs, Azure TTS, Google TTS, etc.)
        // - Save audio file to shared volume
        // - Return audio file path
    }

    private async Task<string> AssembleAndUploadVideoAsync(
        string script, 
        SeoMetadata? seoMetadata, 
        List<VisualPrompt>? visualPrompts,
        Channel channel)
    {
        _logger.LogInformation("Assembling video with audio, visuals, and metadata");
        
        // Simulate video assembly and upload
        await Task.Delay(2000);

        // In real implementation:
        // 1. Use FFmpeg to combine:
        //    - Audio file (from GenerateAudioAsync)
        //    - Images (generated from visualPrompts or stock images)
        //    - Transitions, effects, branding
        // 2. Apply SEO metadata (title, description, tags)
        // 3. Upload to YouTube/TikTok using channel credentials
        // 4. Return actual video URL

        var videoId = Guid.NewGuid().ToString("N").Substring(0, 11);
        var platform = channel.Platform.ToLower();

        if (platform == "youtube")
        {
            return $"https://youtube.com/watch?v={videoId}";
        }
        else if (platform == "tiktok")
        {
            return $"https://tiktok.com/@{channel.Name}/video/{videoId}";
        }

        return $"https://{platform}.com/videos/{videoId}";
    }
}
