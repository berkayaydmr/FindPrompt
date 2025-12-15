﻿using LearnPrompt.Application.Repositories;
using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;

    public CourseService(ICourseRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Course>> GetMyCoursesAsync(string ownerId)
        => _repo.GetByOwnerAsync(ownerId);

    public Task<Course?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public async Task CreateCourseAsync(string ownerId, string title, string? description, string language)
    {
        var course = new Course
        {
            OwnerId = ownerId,
            Title = title,
            Description = description,
            Language = string.IsNullOrWhiteSpace(language) ? "tr" : language
        };

        await _repo.AddAsync(course);
        await _repo.SaveChangesAsync();
    }
    public async Task AddCourseFileAsync(string ownerId, int courseId, string originalFileName, string storedFileName, long fileSize)
    {
        var course = await _repo.GetByIdAsync(courseId);
        if (course == null) throw new Exception("Course not found");
        if (course.OwnerId != ownerId) throw new UnauthorizedAccessException();

        var file = new CourseFile
        {
            CourseId = courseId,
            FileName = originalFileName,
            StoredFileName = storedFileName,
            FileSize = fileSize,
            Status = "Pending"
        };

        await _repo.AddFileAsync(file);
        await _repo.SaveChangesAsync();
    }

    public async Task<CourseFile> DeleteCourseFileAsync(string ownerId, int fileId)
    {
        var file = await _repo.GetFileByIdAsync(fileId) ?? throw new Exception("File not found");
        var course = await _repo.GetByIdAsync(file.CourseId) ?? throw new Exception("Course not found");
        if (course.OwnerId != ownerId) throw new UnauthorizedAccessException();

        await _repo.RemoveFileAsync(file);
        await _repo.SaveChangesAsync();
        return file;
    }

}
