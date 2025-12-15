using LearnPrompt.Application.Repositories;
using LearnPrompt.Domain.Entities;
using LearnPrompt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace LearnPrompt.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Course>> GetByOwnerAsync(string ownerId)
    {
        return _context.Courses
            .Include(c => c.Files)
            .Include(c => c.Topics)
            .Where(c => c.OwnerId == ownerId)
            .ToListAsync();
    }

    public Task<Course?> GetByIdAsync(int id)
    {
        return _context.Courses
            .Include(c => c.Topics)
            .Include(c => c.Files)
            .FirstOrDefaultAsync(c => c.Id == id);
    }


    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
    public async Task AddFileAsync(CourseFile file)
    {
        await _context.CourseFiles.AddAsync(file);
    }

    public Task<List<CourseFile>> GetFilesByCourseIdAsync(int courseId)
    {
        return _context.CourseFiles
            .Where(x => x.CourseId == courseId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public Task<CourseFile?> GetFileByIdAsync(int fileId)
        => _context.CourseFiles.FirstOrDefaultAsync(x => x.Id == fileId);

    public Task RemoveFileAsync(CourseFile file)
    {
        _context.CourseFiles.Remove(file);
        return Task.CompletedTask;
    }


}
