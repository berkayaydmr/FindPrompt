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

    }
}
