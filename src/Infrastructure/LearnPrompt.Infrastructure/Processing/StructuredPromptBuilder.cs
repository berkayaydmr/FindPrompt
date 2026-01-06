using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Processing;
using LearnPrompt.Application.Prompts;
using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Infrastructure.Processing
{
    public class StructuredPromptBuilder : IPromptBuilder
    {
        private readonly IVectorStore _vectorStore;

        public StructuredPromptBuilder(IVectorStore vectorStore)
        {
            _vectorStore = vectorStore;
        }

        public async Task<PromptResult> BuildPromptAsync(
            PromptRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Topic'e bağlı doğrudan chunk'ları getir
            var directChunks = request.Topic.RelatedChunks
                .OrderBy(tc => tc.Order)
                .Select(tc => new VectorSearchResult(tc.Chunk.RawText, tc.Relevance, tc.ChunkId))
                .ToList();
            
            // Ek vector search (kalan slot kadar)
            var additionalCount = Math.Max(0, request.TopK - directChunks.Count);
            var vectorResults = additionalCount > 0 
                ? await _vectorStore.SearchAsync(
                    request.Course.Id,
                    request.Topic.Title,
                    additionalCount,
                    cancellationToken)
                : new List<VectorSearchResult>();

            // Birleştir
            var references = directChunks.Concat(vectorResults).ToList();

            var prompt = BuildPromptText(request, references);
            return new PromptResult(
                request.Course.Id,
                request.Course.Title,
                request.Course.Description,
                request.Topic.Id,
                request.Topic.Title,
                request.Topic.Source,
                prompt,
                references);
        }

        private static string BuildPromptText(
            PromptRequest request,
            IReadOnlyList<VectorSearchResult> references)
        {
            var builder = new StringBuilder();

            builder.AppendLine("You are an expert instructor creating a personalized lesson for the learner.");
            builder.AppendLine("Use only the provided course content and follow the teaching guidelines.");
            builder.AppendLine();

            builder.AppendLine($"Course Title: {request.Course.Title}");
            if (!string.IsNullOrWhiteSpace(request.Course.Description))
            {
                builder.AppendLine($"Course Summary: {request.Course.Description}");
            }

            builder.AppendLine($"Focus Topic: {request.Topic.Title}");

            if (!string.IsNullOrWhiteSpace(request.Topic.Source))
            {
                builder.AppendLine($"Topic Source: {request.Topic.Source}");
            }

            if (!string.IsNullOrWhiteSpace(request.UserName) || !string.IsNullOrWhiteSpace(request.UserLevel))
            {
                builder.AppendLine();
                builder.AppendLine("Learner Profile:");
                if (!string.IsNullOrWhiteSpace(request.UserName))
                {
                    builder.AppendLine($"- Name: {request.UserName}");
                }
                if (!string.IsNullOrWhiteSpace(request.UserLevel))
                {
                    builder.AppendLine($"- Level: {request.UserLevel}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("Teaching Guidelines:");
            builder.AppendLine("- Explain the topic using only the provided course materials.");
            builder.AppendLine("- Break explanations into short, clear sections.");
            builder.AppendLine("- Provide relatable examples.");
            builder.AppendLine("- Pause after each major point to ask a reflective question.");
            builder.AppendLine("- Summarize the key takeaways at the end.");

            if (references.Count == 0)
            {
                builder.AppendLine();
                builder.AppendLine("No relevant course content snippets were retrieved. Provide a high-level overview of the topic and ask the learner for more specifics if needed.");
                return builder.ToString();
            }

            builder.AppendLine();
            builder.AppendLine("Relevant Course Content Snippets:");
            for (var index = 0; index < references.Count; index++)
            {
                var snippet = NormalizeSnippet(references[index].RawText, 600);
                builder.AppendLine($"[{index + 1}] {snippet}");
            }

            builder.AppendLine();
            builder.AppendLine("Task:");
            builder.AppendLine("Craft a conversational lesson covering the focus topic using the snippets above. Include interactive questions and suggest next steps for practice.");

            return builder.ToString();
        }

        private static string NormalizeSnippet(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.ReplaceLineEndings(" ").Trim();
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized[..maxLength].TrimEnd() + "…";
        }
    }
}

