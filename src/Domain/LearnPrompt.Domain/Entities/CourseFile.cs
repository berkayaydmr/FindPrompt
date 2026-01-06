using System;
using System.Collections.Generic;
using LearnPrompt.Domain.Constants;

namespace LearnPrompt.Domain.Entities
{
    public class CourseFile
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string StoredFileName { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = CourseFileStatus.Pending;
        public DateTime? ProcessedAt { get; set; }
        public string? FailedReason { get; set; }

        public ICollection<ContentChunk> Chunks { get; set; } = new List<ContentChunk>();
    }
}