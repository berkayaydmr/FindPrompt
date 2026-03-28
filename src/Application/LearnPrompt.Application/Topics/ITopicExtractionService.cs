using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Application.Topics
{
    public interface ITopicExtractionService
    {
        Task<IReadOnlyList<TopicSuggestion>> ExtractTopicsAsync(
            Course course,
            IReadOnlyCollection<ContentChunk> chunks,
            CancellationToken cancellationToken = default);
    }
}

