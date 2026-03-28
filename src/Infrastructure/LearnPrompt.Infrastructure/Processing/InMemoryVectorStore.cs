using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Processing;

namespace LearnPrompt.Infrastructure.Processing
{
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentDictionary<Guid, VectorStoreRecord> _storage = new();
        private readonly IEmbeddingService _embeddingService;

        public InMemoryVectorStore(IEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        public Task UpsertChunkEmbeddingAsync(VectorStoreRecord record, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sanitized = new VectorStoreRecord
            {
                ChunkId = record.ChunkId,
                CourseId = record.CourseId,
                CourseFileId = record.CourseFileId,
                RawText = record.RawText,
                Embedding = record.Embedding.ToArray(),
                Metadata = new Dictionary<string, string>(record.Metadata)
            };

            _storage.AddOrUpdate(record.ChunkId, sanitized, (_, _) => sanitized);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(int courseId, string query, int topK, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            var candidates = _storage.Values
                .Where(r => r.CourseId == courseId)
                .ToList();

            var results = candidates
                .Select(record => new VectorSearchResult
                {
                    ChunkId = record.ChunkId,
                    CourseId = record.CourseId,
                    CourseFileId = record.CourseFileId,
                    RawText = record.RawText,
                    Score = CosineSimilarity(queryEmbedding, record.Embedding)
                })
                .OrderByDescending(r => r.Score)
                .Take(topK)
                .ToList();

            return results;
        }

        public Task RemoveByCourseFileAsync(int courseFileId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var keys = _storage
                .Where(kvp => kvp.Value.CourseFileId == courseFileId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keys)
            {
                _storage.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }

        private static float CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
        {
            if (left.Count != right.Count) return 0f;

            double dot = 0;
            double normLeft = 0;
            double normRight = 0;

            for (var i = 0; i < left.Count; i++)
            {
                var l = left[i];
                var r = right[i];

                dot += l * r;
                normLeft += l * l;
                normRight += r * r;
            }

            if (normLeft == 0 || normRight == 0) return 0f;

            return (float)(dot / (Math.Sqrt(normLeft) * Math.Sqrt(normRight)));
        }
    }
}

