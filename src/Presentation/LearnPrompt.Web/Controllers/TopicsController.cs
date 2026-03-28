using System;
using System.Linq;
using System.Threading.Tasks;
using LearnPrompt.Application.Topics;
using LearnPrompt.Domain.Constants;
using LearnPrompt.Web.Models.Topics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnPrompt.Web.Controllers
{
    [Authorize]
    public class TopicsController : Controller
    {
        private readonly ICourseTopicService _courseTopicService;
        private readonly UserManager<IdentityUser> _userManager;

        public TopicsController(
            ICourseTopicService courseTopicService,
            UserManager<IdentityUser> userManager)
        {
            _courseTopicService = courseTopicService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                var course = await _courseTopicService.GetCourseWithTopicsAsync(courseId, userId);

                var viewModel = new TopicIndexViewModel
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    CourseDescription = course.Description,
                    HasChunks = course.Chunks?.Any() ?? false,
                    Topics = course.CourseTopics
                        .Select(t => new TopicItemViewModel
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Source = t.Source,
                            IsManual = t.IsManual,
                            IsSelected = t.IsSelected,
                            Confidence = t.Confidence
                        })
                        .ToList()
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                var topics = await _courseTopicService.GenerateTopicsAsync(courseId, userId);
                TempData["StatusMessage"] = topics.Count == 0
                    ? "İşlenmiş içerik bulunamadı veya konu başlığı üretilemedi."
                    : $"{topics.Count} konu başlığı oluşturuldu.";
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Konu başlıkları oluşturulurken bir hata meydana geldi.";
            }

            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, string title)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                await _courseTopicService.CreateManualTopicAsync(courseId, userId, title);
                TempData["StatusMessage"] = "Konu başlığı eklendi.";
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSelection(int courseId, int topicId, bool isSelected)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                await _courseTopicService.SetTopicSelectionAsync(topicId, userId, isSelected);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int courseId, int topicId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                await _courseTopicService.DeleteTopicAsync(topicId, userId);
                TempData["StatusMessage"] = "Konu başlığı silindi.";
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}

