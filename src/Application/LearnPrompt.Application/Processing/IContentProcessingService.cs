using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Processing
{
    public interface IContentProcessingService
    {
        Task ProcessCourseFileAsync(int courseFileId, CancellationToken cancellationToken = default);
    }
}

