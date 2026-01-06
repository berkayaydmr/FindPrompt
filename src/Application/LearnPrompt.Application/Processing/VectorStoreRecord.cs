using System;
using System.Collections.Generic;

namespace LearnPrompt.Application.Processing
{
    public class VectorStoreRecord
    {
        public Guid ChunkId { get; init; }
        public int CourseId { get; init; }
        public int CourseFileId { get; init; }
        public float[] Embedding { get; init; } = Array.Empty<float>();
        public string RawText { get; init; } = string.Empty;
        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
    }
}

