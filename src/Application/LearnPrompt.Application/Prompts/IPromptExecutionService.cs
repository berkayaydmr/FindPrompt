using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Prompts
{
    public interface IPromptExecutionService
    {
        Task<PromptExecutionResult> ExecuteAsync(
            int topicId,
            string ownerId,
            string? userName,
            string? userLevel,
            int topK = 6,
            CancellationToken cancellationToken = default);
    }
}

