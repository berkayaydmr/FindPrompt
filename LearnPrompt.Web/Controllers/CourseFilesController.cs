using LearnPrompt.Application.Services;
using LearnPrompt.Domain.Entities;
using LearnPrompt.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
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

    public CourseFilesController(
        ICourseService courseService,
        AppDbContext db,
        UserManager<IdentityUser> userManager,
        IWebHostEnvironment env)
    {
        _courseService = courseService;
        _db = db;
        _userManager = userManager;
        _env = env;
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

            _db.CourseFiles.Add(new CourseFile
            {
                CourseId = courseId,
                FileName = originalName,
                StoredFileName = storedName,
                FileSize = file.Length,
                Status = "Pending",
                UploadedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var file = await _courseService.DeleteCourseFileAsync(userId, id);
        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", file.CourseId.ToString());
        var storedPath = Path.Combine(uploadsRoot, file.StoredFileName);

        if (System.IO.File.Exists(storedPath))
        {
            System.IO.File.Delete(storedPath);
        }

        return RedirectToAction("Details", "Courses", new { id = file.CourseId });
    }
}
