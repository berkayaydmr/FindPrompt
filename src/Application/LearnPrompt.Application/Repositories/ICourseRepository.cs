using LearnPrompt.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Repositories
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetByOwnerAsync(string ownerId);
        Task<Course?> GetByIdAsync(int id);
        Task AddAsync(Course course);
        Task SaveChangesAsync();
        Task AddFileAsync(CourseFile file);
        Task<List<CourseFile>> GetFilesByCourseIdAsync(int courseId);
        Task<CourseFile?> GetFileByIdAsync(int fileId);
        Task<CourseFile?> GetFileWithCourseAsync(int fileId);
        Task AddChunksAsync(IEnumerable<ContentChunk> chunks);
        Task<List<ContentChunk>> GetChunksByCourseIdAsync(int courseId);
        Task RemoveChunksByCourseFileIdAsync(int courseFileId);
        Task RemoveFileAsync(CourseFile file);
        Task AddTopicsAsync(IEnumerable<CourseTopic> topics);
        Task<List<CourseTopic>> GetTopicsByCourseIdAsync(int courseId);
        Task<CourseTopic?> GetTopicByIdAsync(int topicId);
        void RemoveTopics(IEnumerable<CourseTopic> topics);
        Task RemoveTopicAsync(CourseTopic topic);
    }
}
