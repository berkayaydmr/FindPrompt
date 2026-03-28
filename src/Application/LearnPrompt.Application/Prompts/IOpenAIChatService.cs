using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Prompts
{
    public interface IOpenAIChatService
    {
        Task<string> GenerateCompletionAsync(string prompt, CancellationToken cancellationToken = default);
    }
}

