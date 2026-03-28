using System.Collections.Generic;
using LearnPrompt.Application.Services;
using LearnPrompt.Domain.Entities;
using LearnPrompt.Infrastructure.Data;
using LearnPrompt.Application.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnPrompt.Web.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class CourseFilesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly IContentProcessingService _contentProcessingService;
    private readonly IVectorStore _vectorStore;

    public CourseFilesController(
        ICourseService courseService,
        AppDbContext db,
        UserManager<IdentityUser> userManager,
        IWebHostEnvironment env,
        IContentProcessingService contentProcessingService,
        IVectorStore vectorStore)
    {
        _courseService = courseService;
        _db = db;
        _userManager = userManager;
        _env = env;
        _contentProcessingService = contentProcessingService;
        _vectorStore = vectorStore;
    }

    
    [HttpGet]
    public async Task<IActionResult> Upload([FromQuery] int courseId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var course = await _courseService.GetByIdAsync(courseId);
        if (course == null) return NotFound();
        if (course.OwnerId != userId) return Forbid();

        ViewBag.CourseId = courseId;
        ViewBag.CourseTitle = course.Title;
        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload([FromQuery] int courseId, List<IFormFile> files)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var course = await _courseService.GetByIdAsync(courseId);
        if (course == null) return NotFound();
        if (course.OwnerId != userId) return Forbid();

        if (files == null || files.Count == 0)
        {
            ModelState.AddModelError("", "Please select at least one file.");
            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;
            return View();
        }

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", courseId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var createdFiles = new List<CourseFile>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var originalName = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(originalName);
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsRoot, storedName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var courseFile = new CourseFile
            {
                CourseId = courseId,
                FileName = originalName,
                StoredFileName = storedName,
                FileSize = file.Length,
                Status = Domain.Constants.CourseFileStatus.Pending,
                UploadedAt = DateTime.UtcNow
            };

            _db.CourseFiles.Add(courseFile);
            createdFiles.Add(courseFile);
        }

        await _db.SaveChangesAsync();

        foreach (var courseFile in createdFiles)
        {
            await _contentProcessingService.ProcessCourseFileAsync(courseFile.Id);
        }

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        CourseFile file;
        try
        {
            file = await _courseService.DeleteCourseFileAsync(userId, id);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        await _vectorStore.RemoveByCourseFileAsync(file.Id);

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", file.CourseId.ToString());
        var storedPath = Path.Combine(uploadsRoot, file.StoredFileName);

        if (System.IO.File.Exists(storedPath))
        {
            System.IO.File.Delete(storedPath);
        }

        return RedirectToAction("Details", "Courses", new { id = file.CourseId });
    }
}
