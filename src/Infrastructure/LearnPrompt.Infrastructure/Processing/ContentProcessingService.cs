using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Processing;
using LearnPrompt.Application.Repositories;
using LearnPrompt.Domain.Constants;
using LearnPrompt.Domain.Entities;
using LearnPrompt.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace LearnPrompt.Infrastructure.Processing
{
    public class ContentProcessingService : IContentProcessingService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ITextExtractionService _textExtractionService;
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly ILogger<ContentProcessingService> _logger;
        private readonly string _uploadsRoot;

        public ContentProcessingService(
            ICourseRepository courseRepository,
            ITextExtractionService textExtractionService,
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            ILogger<ContentProcessingService> logger,
            IWebHostEnvironment environment)
        {
            _courseRepository = courseRepository;
            _textExtractionService = textExtractionService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _logger = logger;
            _uploadsRoot = Path.Combine(environment.WebRootPath, "uploads");
        }

        public async Task ProcessCourseFileAsync(int courseFileId, CancellationToken cancellationToken = default)
        {
            var file = await _courseRepository.GetFileWithCourseAsync(courseFileId)
                ?? throw new KeyNotFoundException($"Course file {courseFileId} was not found.");

            if (file.Status == CourseFileStatus.Processing)
            {
                _logger.LogInformation("Course file {FileId} is already being processed.", courseFileId);
                return;
            }

            if (file.Status == CourseFileStatus.Completed && file.Chunks.Any())
            {
                _logger.LogInformation("Course file {FileId} already completed. Skipping.", courseFileId);
                return;
            }

            var physicalPath = Path.Combine(_uploadsRoot, file.CourseId.ToString(), file.StoredFileName);
            if (!System.IO.File.Exists(physicalPath))
            {
                await MarkAsFailed(file, $"File not found at path {physicalPath}");
                return;
            }

            file.Status = CourseFileStatus.Processing;
            file.FailedReason = null;
            await _courseRepository.SaveChangesAsync();

            try
            {
                var extracted = await _textExtractionService
                    .ExtractTextAsync(physicalPath, file.FileName, cancellationToken);

                if (string.IsNullOrWhiteSpace(extracted))
                {
                    await MarkAsFailed(file, "No textual content could be extracted from the document.");
                    return;
                }

                await _courseRepository.RemoveChunksByCourseFileIdAsync(file.Id);

                var chunks = _chunkingService
                    .SplitIntoChunks(extracted)
                    .Select((chunkText, index) => new ContentChunk
                    {
                        Id = Guid.NewGuid(),
                        CourseId = file.CourseId,
                        CourseFileId = file.Id,
                        OrderIndex = index,
                        RawText = chunkText
                    })
                    .ToList();

                if (chunks.Count == 0)
                {
                    await MarkAsFailed(file, "Chunking produced no output.");
                    return;
                }

                await _courseRepository.AddChunksAsync(chunks);
                await _courseRepository.SaveChangesAsync();

                foreach (var chunk in chunks)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.RawText, cancellationToken);

                    var record = new VectorStoreRecord
                    {
                        ChunkId = chunk.Id,
                        CourseId = chunk.CourseId,
                        CourseFileId = chunk.CourseFileId,
                        RawText = chunk.RawText,
                        Embedding = embedding,
                        Metadata = new Dictionary<string, string>
                        {
                            ["courseId"] = chunk.CourseId.ToString(),
                            ["courseFileId"] = chunk.CourseFileId.ToString(),
                            ["orderIndex"] = chunk.OrderIndex.ToString()
                        }
                    };

                    await _vectorStore.UpsertChunkEmbeddingAsync(record, cancellationToken);
                }

                file.Status = CourseFileStatus.Completed;
                file.ProcessedAt = DateTime.UtcNow;
                file.FailedReason = null;
                await _courseRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully processed course file {FileId}", file.Id);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Processing for course file {FileId} was cancelled.", file.Id);
                throw;
            }
            catch (Exception ex)
            {
                await MarkAsFailed(file, ex.Message);
                _logger.LogError(ex, "Processing failed for course file {FileId}", file.Id);
            }
        }

        private async Task MarkAsFailed(CourseFile file, string reason)
        {
            file.Status = CourseFileStatus.Failed;
            file.ProcessedAt = null;
            file.FailedReason = reason.Length > 500
                ? reason[..500]
                : reason;
            await _courseRepository.SaveChangesAsync();
        }
    }
}

