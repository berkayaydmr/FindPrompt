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
            .ThenInclude(f => f.Chunks)
            .Include(c => c.Topics)
            .Include(c => c.CourseTopics)
            .Include(c => c.Chunks)
            .Where(c => c.OwnerId == ownerId)
            .ToListAsync();
    }

    public Task<Course?> GetByIdAsync(int id)
    {
        return _context.Courses
            .Include(c => c.Topics)
            .Include(c => c.CourseTopics)
            .Include(c => c.Files)
                .ThenInclude(f => f.Chunks)
            .Include(c => c.Chunks)
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
            .Include(x => x.Chunks)
            .Include(x => x.Course.CourseTopics)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public Task<CourseFile?> GetFileByIdAsync(int fileId)
        => _context.CourseFiles
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(x => x.Id == fileId);

    public Task<CourseFile?> GetFileWithCourseAsync(int fileId)
        => _context.CourseFiles
            .Include(x => x.Course)
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(x => x.Id == fileId);

    public async Task AddChunksAsync(IEnumerable<ContentChunk> chunks)
    {
        await _context.ContentChunks.AddRangeAsync(chunks);
    }

    public Task<List<ContentChunk>> GetChunksByCourseIdAsync(int courseId)
        => _context.ContentChunks
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync();

    public Task RemoveChunksByCourseFileIdAsync(int courseFileId)
    {
        var existing = _context.ContentChunks.Where(x => x.CourseFileId == courseFileId);
        _context.ContentChunks.RemoveRange(existing);
        return Task.CompletedTask;
    }

    public Task RemoveFileAsync(CourseFile file)
    {
        _context.CourseFiles.Remove(file);
        return Task.CompletedTask;
    }

    public async Task AddTopicsAsync(IEnumerable<CourseTopic> topics)
    {
        await _context.CourseTopics.AddRangeAsync(topics);
    }

    public Task<List<CourseTopic>> GetTopicsByCourseIdAsync(int courseId)
        => _context.CourseTopics
            .Where(t => t.CourseId == courseId)
            .OrderByDescending(t => t.IsSelected)
            .ThenBy(t => t.Source)
            .ThenBy(t => t.Title)
            .ToListAsync();

    public Task<CourseTopic?> GetTopicByIdAsync(int topicId)
        => _context.CourseTopics
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == topicId);

    public void RemoveTopics(IEnumerable<CourseTopic> topics)
    {
        _context.CourseTopics.RemoveRange(topics);
    }

    public Task RemoveTopicAsync(CourseTopic topic)
    {
        _context.CourseTopics.Remove(topic);
        return Task.CompletedTask;
    }


}
