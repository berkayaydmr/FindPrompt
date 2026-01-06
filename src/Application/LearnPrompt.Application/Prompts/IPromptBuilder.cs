using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Prompts
{
    public interface IPromptBuilder
    {
        Task<PromptResult> BuildPromptAsync(PromptRequest request, CancellationToken cancellationToken = default);
    }
}

