using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnPrompt.Domain.Entities
{
    public class LessonTopic
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        public int Order { get; set; }
        
        public ICollection<TopicContentChunk> RelatedChunks { get; set; } = new List<TopicContentChunk>();
    }
}

