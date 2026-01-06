using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Application.Services;

public interface ICourseService
{
    Task<List<Course>> GetMyCoursesAsync(string ownerId);
    Task<Course?> GetByIdAsync(int id);
    Task CreateCourseAsync(string ownerId, string title, string? description, string language);
    Task AddCourseFileAsync(string ownerId, int courseId, string originalFileName, string storedFileName, long fileSize);
    Task<CourseFile> DeleteCourseFileAsync(string ownerId, int fileId);

}
