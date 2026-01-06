using System;
using System.Collections.Generic;

namespace LearnPrompt.Domain.Entities
{
    public class ContentChunk
    {
        public Guid Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public int CourseFileId { get; set; }
        public CourseFile CourseFile { get; set; } = null!;
        public int OrderIndex { get; set; }
        public string RawText { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

