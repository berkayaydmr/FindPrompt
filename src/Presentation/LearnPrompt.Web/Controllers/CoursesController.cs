using LearnPrompt.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnPrompt.Web.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly UserManager<IdentityUser> _userManager;

    public CoursesController(ICourseService courseService, UserManager<IdentityUser> userManager)
    {
        _courseService = courseService;
        _userManager = userManager;
    }

    
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var courses = await _courseService.GetMyCoursesAsync(userId);
        return View(courses);
    }

    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, string? description, string? language)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(title), "Title is required.");
            return View();
        }

        await _courseService.CreateCourseAsync(userId, title, description, language ?? "tr");
        return RedirectToAction(nameof(Index));
    }

    
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var course = await _courseService.GetByIdAsync(id);
        if (course == null) return NotFound();

        if (course.OwnerId != userId) return Forbid(); 

        return View(course);
    }
}
