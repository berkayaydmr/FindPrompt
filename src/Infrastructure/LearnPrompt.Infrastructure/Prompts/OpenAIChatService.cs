using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Prompts;
using LearnPrompt.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace LearnPrompt.Infrastructure.Prompts
{
    public class OpenAIChatService : IOpenAIChatService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;

        public OpenAIChatService(HttpClient httpClient, IOptions<OpenAIOptions> optionsAccessor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        }

        public async Task<string> GenerateCompletionAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (prompt is null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured. Please provide it via configuration.");
            }

            var model = string.IsNullOrWhiteSpace(_options.ChatModel)
                ? _options.Model
                : _options.ChatModel;

            var temperature = double.IsNaN(_options.ChatTemperature)
                ? _options.Temperature
                : _options.ChatTemperature;

            var requestPayload = new ChatCompletionsRequest
            {
                Model = model,
                Messages = new List<ChatMessage>
                {
                    new ChatMessage("system", "You are an expert educator crafting helpful, structured responses."),
                    new ChatMessage("user", prompt)
                },
                Temperature = temperature
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(requestPayload)
            };

            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ChatCompletionsResponse>(cancellationToken: cancellationToken);
            if (payload?.Choices is null || payload.Choices.Count == 0)
            {
                throw new InvalidOperationException("OpenAI chat completion did not return any results.");
            }

            var content = payload.Choices[0].Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("OpenAI chat completion returned an empty response.");
            }

            return content.Trim();
        }

        private record ChatCompletionsRequest
        {
            [JsonPropertyName("model")]
            public string? Model { get; init; }

            [JsonPropertyName("messages")]
            public List<ChatMessage> Messages { get; init; } = new();

            [JsonPropertyName("temperature")]
            public double? Temperature { get; init; }
        }

        private record ChatMessage(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content")] string Content);

        private record ChatChoice(
            [property: JsonPropertyName("index")] int Index,
            [property: JsonPropertyName("message")] ChatMessage? Message);

        private record ChatCompletionsResponse(
            [property: JsonPropertyName("choices")] List<ChatChoice>? Choices);
    }
}

