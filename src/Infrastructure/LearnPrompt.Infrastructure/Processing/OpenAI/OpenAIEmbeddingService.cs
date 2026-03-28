using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Processing;
using LearnPrompt.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace LearnPrompt.Infrastructure.Processing.OpenAI
{
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;

        public OpenAIEmbeddingService(
            HttpClient httpClient,
            IOptions<OpenAIOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<float[]> GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }

            var request = new
            {
                input = text,
                model = "text-embedding-3-small", // 1536 dimensions
                encoding_format = "float"
            };

            var response = await _httpClient.PostAsJsonAsync(
                "embeddings",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
                cancellationToken: cancellationToken);

            if (result?.Data == null || result.Data.Count == 0)
            {
                throw new InvalidOperationException("No embedding data received from OpenAI");
            }

            return result.Data[0].Embedding;
        }

        private class EmbeddingResponse
        {
            [JsonPropertyName("data")]
            public System.Collections.Generic.List<EmbeddingData> Data { get; set; } = new();

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("usage")]
            public UsageData Usage { get; set; } = new();
        }

        private class EmbeddingData
        {
            [JsonPropertyName("embedding")]
            public float[] Embedding { get; set; } = Array.Empty<float>();

            [JsonPropertyName("index")]
            public int Index { get; set; }
        }

        private class UsageData
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}

