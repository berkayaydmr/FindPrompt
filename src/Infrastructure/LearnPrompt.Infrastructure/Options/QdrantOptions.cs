namespace LearnPrompt.Infrastructure.Options
{
    public class QdrantOptions
    {
        public const string SectionName = "Qdrant";
        
        public string Endpoint { get; set; } = "http://localhost:6334";
        public string? ApiKey { get; set; }
        public string CollectionName { get; set; } = "course_embeddings";
        public int VectorSize { get; set; } = 1536; // OpenAI text-embedding-3-small default size
    }
}

