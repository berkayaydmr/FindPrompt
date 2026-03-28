using System.Collections.Generic;

namespace LearnPrompt.Application.Topics
{
    public sealed record TopicSuggestion(
        string Title,
        double Confidence = 0.0,
        IReadOnlyCollection<int>? SupportingChunkOrderIndexes = null);
}

