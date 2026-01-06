using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Application.Topics
{
    public interface ICourseTopicService
    {
        Task<Course> GetCourseWithTopicsAsync(int courseId, string ownerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CourseTopic>> GenerateTopicsAsync(int courseId, string ownerId, CancellationToken cancellationToken = default);
        Task<CourseTopic> CreateManualTopicAsync(int courseId, string ownerId, string title, CancellationToken cancellationToken = default);
        Task<CourseTopic> SetTopicSelectionAsync(int topicId, string ownerId, bool isSelected, CancellationToken cancellationToken = default);
        Task DeleteTopicAsync(int topicId, string ownerId, CancellationToken cancellationToken = default);
    }
}

