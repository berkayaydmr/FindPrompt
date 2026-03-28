using System.Collections.Generic;
using LearnPrompt.Application.Processing;

namespace LearnPrompt.Application.Prompts
{
    public record PromptResult(
        int CourseId,
        string CourseTitle,
        string? CourseDescription,
        int TopicId,
        string TopicTitle,
        string TopicSource,
        string Prompt,
        IReadOnlyList<VectorSearchResult> References);
}

