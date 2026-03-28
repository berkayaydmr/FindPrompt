using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Processing
{
    public interface ITextExtractionService
    {
        Task<string> ExtractTextAsync(string filePath, string fileName, CancellationToken cancellationToken = default);
    }
}

