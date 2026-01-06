using System;

namespace LearnPrompt.Application.Processing
{
    public class VectorSearchResult
    {
        public Guid ChunkId { get; init; }
        public float Score { get; init; }
        public string RawText { get; init; } = string.Empty;
        public int CourseId { get; init; }
        public int CourseFileId { get; init; }
    }
}

