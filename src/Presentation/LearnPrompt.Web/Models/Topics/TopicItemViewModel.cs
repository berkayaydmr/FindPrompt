namespace LearnPrompt.Web.Models.Topics
{
    public class TopicItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsManual { get; set; }
        public double? Confidence { get; set; }
    }
}

