using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LearnPrompt.Domain.Entities
{
    public class Course
    {
        public int Id { get; set; }


        [Required]
        public string OwnerId { get; set; } = null!;

        public IdentityUser? Owner { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Language { get; set; } = "tr";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CourseFile> Files { get; set; } = new List<CourseFile>();
        public ICollection<ContentChunk> Chunks { get; set; } = new List<ContentChunk>();
        public ICollection<CourseTopic> CourseTopics { get; set; } = new List<CourseTopic>();
        public ICollection<LessonTopic> Topics { get; set; } = new List<LessonTopic>();
    }
}
