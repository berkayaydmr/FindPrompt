using System.Collections.Generic;

namespace LearnPrompt.Web.Models.Topics
{
    public class TopicIndexViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseDescription { get; set; }
        public IReadOnlyList<TopicItemViewModel> Topics { get; set; } = new List<TopicItemViewModel>();
        public bool HasChunks { get; set; }
    }
}

