using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Prompts
{
    public interface IPromptService
    {
        Task<PromptResult> BuildPromptForTopicAsync(int topicId, string ownerId, string? userName, string? userLevel, int topK = 6, CancellationToken cancellationToken = default);
    }
}

