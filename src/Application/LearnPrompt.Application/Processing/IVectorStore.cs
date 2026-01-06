using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Processing
{
    public interface IVectorStore
    {
        Task UpsertChunkEmbeddingAsync(VectorStoreRecord record, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VectorSearchResult>> SearchAsync(int courseId, string query, int topK, CancellationToken cancellationToken = default);
        Task RemoveByCourseFileAsync(int courseFileId, CancellationToken cancellationToken = default);
    }
}

