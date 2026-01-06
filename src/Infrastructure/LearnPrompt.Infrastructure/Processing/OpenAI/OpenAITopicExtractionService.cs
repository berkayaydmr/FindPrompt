using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Topics;
using LearnPrompt.Domain.Entities;
using LearnPrompt.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LearnPrompt.Infrastructure.Processing.OpenAI
{
    public class OpenAITopicExtractionService : ITopicExtractionService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;
        private readonly ILogger<OpenAITopicExtractionService> _logger;

        public OpenAITopicExtractionService(
            HttpClient httpClient,
            IOptions<OpenAIOptions> options,
            ILogger<OpenAITopicExtractionService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<TopicSuggestion>> ExtractTopicsAsync(
            Course course,
            IReadOnlyCollection<ContentChunk> chunks,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is not configured. Please provide a key via user secrets or environment variables.");
            }

            if (course == null) throw new ArgumentNullException(nameof(course));
            if (chunks == null) throw new ArgumentNullException(nameof(chunks));
            if (chunks.Count == 0) return Array.Empty<TopicSuggestion>();

            var payload = BuildRequestPayload(course, chunks);
            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI topic extraction failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new InvalidOperationException("OpenAI topic extraction request failed.");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseResponse(json);
        }

        private object BuildRequestPayload(Course course, IReadOnlyCollection<ContentChunk> chunks)
        {
            var snippets = chunks
                .OrderBy(c => c.OrderIndex)
                .Take(12)
                .Select((chunk, index) => new
                {
                    index,
                    text = Truncate(chunk.RawText, 800)
                })
                .ToList();

            var userPrompt = new StringBuilder();
            userPrompt.AppendLine("Analyze the following course material snippets and propose between 5 and 10 concise study topics.");
            userPrompt.AppendLine("Return a JSON object with a `topics` array. Each array item must contain:");
            userPrompt.AppendLine("{\"title\": string, \"confidence\": number (0-1), \"supportingChunkIndexes\": number[]}.");
            userPrompt.AppendLine("Titles must be unique, actionable, and under 80 characters.");
            userPrompt.AppendLine();
            userPrompt.AppendLine($"Course Title: {course.Title}");
            if (!string.IsNullOrWhiteSpace(course.Description))
            {
                userPrompt.AppendLine($"Course Description: {Truncate(course.Description!, 240)}");
            }
            userPrompt.AppendLine("Content Snippets:");
            foreach (var snippet in snippets)
            {
                userPrompt.AppendLine($"- Chunk {snippet.index}: {snippet.text}");
            }

            return new
            {
                model = _options.Model,
                temperature = _options.Temperature,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = "You are an educational designer that extracts study topics from course materials." },
                    new { role = "user", content = userPrompt.ToString() }
                }
            };
        }

        private static IReadOnlyList<TopicSuggestion> ParseResponse(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var content = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                return Array.Empty<TopicSuggestion>();
            }

            using var topicsDocument = JsonDocument.Parse(content);
            if (!topicsDocument.RootElement.TryGetProperty("topics", out var topicsElement) || topicsElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<TopicSuggestion>();
            }

            var results = new List<TopicSuggestion>();
            foreach (var topicElement in topicsElement.EnumerateArray())
            {
                if (!topicElement.TryGetProperty("title", out var titleElement)) continue;

                var title = titleElement.GetString();
                if (string.IsNullOrWhiteSpace(title)) continue;

                double confidence = 0.0;
                if (topicElement.TryGetProperty("confidence", out var confidenceElement) &&
                    confidenceElement.TryGetDouble(out var confidenceValue))
                {
                    confidence = Math.Clamp(confidenceValue, 0, 1);
                }

                IReadOnlyCollection<int>? indexes = null;
                if (topicElement.TryGetProperty("supportingChunkIndexes", out var indexElement) &&
                    indexElement.ValueKind == JsonValueKind.Array)
                {
                    indexes = indexElement
                        .EnumerateArray()
                        .Select(e => e.TryGetInt32(out var value) ? value : (int?)null)
                        .Where(v => v.HasValue)
                        .Select(v => v!.Value)
                        .Distinct()
                        .ToArray();
                }

                results.Add(new TopicSuggestion(title.Trim(), confidence, indexes));
            }

            return results;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength] + "…";
        }
    }
}

