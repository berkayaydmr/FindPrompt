using LearnPrompt.Application.Repositories;
using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Application.Services;

public interface ICourseService
{
    Task<List<Course>> GetMyCoursesAsync(string ownerId);
    Task CreateCourseAsync(string ownerId, string title, string? description);
}

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;

    public CourseService(ICourseRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Course>> GetMyCoursesAsync(string ownerId)
        => _repo.GetByOwnerAsync(ownerId);

    public async Task CreateCourseAsync(string ownerId, string title, string? description)
    {
        var course = new Course
        {
            OwnerId = ownerId,
            Title = title,
            Description = description
        };

        await _repo.AddAsync(course);
        await _repo.SaveChangesAsync();
    }
}
