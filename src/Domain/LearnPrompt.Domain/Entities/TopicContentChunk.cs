using System;

namespace LearnPrompt.Domain.Entities
{
    public class TopicContentChunk
    {
        public int TopicId { get; set; }
        public CourseTopic Topic { get; set; } = null!;
        
        public Guid ChunkId { get; set; }
        public ContentChunk Chunk { get; set; } = null!;
        
        public float Relevance { get; set; }
        public int Order { get; set; }
    }
}

