using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TubeOrchestrator.Core.AI;

namespace TubeOrchestrator.Infrastructure.AI;

/// <summary>
/// OpenAI provider implementation (optional backup for text generation)
/// </summary>
public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIProvider> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://api.openai.com/v1";

    public string ProviderName => "OpenAI";

    public OpenAIProvider(HttpClient httpClient, ILogger<OpenAIProvider> logger, string apiKey)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = apiKey;
    }

    public async Task<string> GenerateTextAsync(string prompt, double temperature = 0.7, int maxTokens = 2000)
    {
        _logger.LogInformation("OpenAI generating text with temperature {Temperature}", temperature);

        try
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant specialized in content creation." },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = maxTokens
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            
            if (result?.Choices != null && result.Choices.Length > 0)
            {
                var generatedText = result.Choices[0].Message?.Content ?? string.Empty;
                _logger.LogInformation("OpenAI generated {Length} characters", generatedText.Length);
                return generatedText;
            }

            throw new InvalidOperationException("OpenAI returned no choices in response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }

    public async Task<string> AnalyzeImageAsync(string imageUrl, string? prompt = null)
    {
        _logger.LogInformation("OpenAI analyzing image: {ImageUrl}", imageUrl);

        try
        {
            var messages = new List<object>
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt ?? "Analyze this image and describe what you see in detail." },
                        new { type = "image_url", image_url = new { url = imageUrl } }
                    }
                }
            };

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = messages,
                max_tokens = 1000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            
            if (result?.Choices != null && result.Choices.Length > 0)
            {
                return result.Choices[0].Message?.Content ?? string.Empty;
            }

            throw new InvalidOperationException("OpenAI returned no choices in response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image with OpenAI");
            throw;
        }
    }

    private class OpenAIResponse
    {
        public Choice[]? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
}
