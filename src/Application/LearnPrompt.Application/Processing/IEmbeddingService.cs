using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Processing
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}

