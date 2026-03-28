using System.Collections.Generic;

namespace LearnPrompt.Web.Models.Prompts
{
    public class PromptViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseDescription { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = string.Empty;
        public string TopicSource { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public IReadOnlyList<PromptReferenceViewModel> References { get; set; } = new List<PromptReferenceViewModel>();
        public string? GeneratedResponse { get; set; }
    }
}

