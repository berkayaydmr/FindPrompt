using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Prompts;
using LearnPrompt.Web.Models.Prompts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnPrompt.Web.Controllers
{
    [Authorize]
    public class PromptsController : Controller
    {
        private readonly IPromptService _promptService;
        private readonly IPromptExecutionService _promptExecutionService;
        private readonly UserManager<IdentityUser> _userManager;

        public PromptsController(
            IPromptService promptService,
            IPromptExecutionService promptExecutionService,
            UserManager<IdentityUser> userManager)
        {
            _promptService = promptService;
            _promptExecutionService = promptExecutionService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Show(int courseId, int topicId, int top = 6, CancellationToken cancellationToken = default)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                var userName = User.Identity?.Name;
                var result = await _promptService.BuildPromptForTopicAsync(
                    topicId,
                    userId,
                    userName,
                    null,
                    Math.Clamp(top, 1, 10),
                    cancellationToken);

                if (result.CourseId != courseId)
                {
                    return NotFound();
                }

                var viewModel = new PromptViewModel
                {
                    CourseId = result.CourseId,
                    CourseTitle = result.CourseTitle,
                    CourseDescription = result.CourseDescription,
                    TopicId = result.TopicId,
                    TopicTitle = result.TopicTitle,
                    TopicSource = result.TopicSource,
                    Prompt = result.Prompt,
                    References = result.References
                        .Select(r => new PromptReferenceViewModel
                        {
                            RawText = r.RawText,
                            Score = Math.Round(r.Score, 3)
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
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Kişisel içerik hazırlanırken bir hata oluştu.";
            }

            return RedirectToAction("Index", "Topics", new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Execute(int courseId, int topicId, int top = 6, CancellationToken cancellationToken = default)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            try
            {
                var execution = await _promptExecutionService.ExecuteAsync(
                    topicId,
                    userId,
                    User.Identity?.Name,
                    null,
                    Math.Clamp(top, 1, 10),
                    cancellationToken);

                if (execution.Prompt.CourseId != courseId)
                {
                    return NotFound();
                }

                var result = execution.Prompt;
                var viewModel = new PromptViewModel
                {
                    CourseId = result.CourseId,
                    CourseTitle = result.CourseTitle,
                    CourseDescription = result.CourseDescription,
                    TopicId = result.TopicId,
                    TopicTitle = result.TopicTitle,
                    TopicSource = result.TopicSource,
                    Prompt = result.Prompt,
                    References = execution.Prompt.References
                        .Select(r => new PromptReferenceViewModel
                        {
                            RawText = r.RawText,
                            Score = Math.Round(r.Score, 3)
                        })
                        .ToList(),
                    GeneratedResponse = execution.Response
                };

                TempData["StatusMessage"] = "LLM yanıtı hazır.";

                return View("Show", viewModel);
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
                TempData["ErrorMessage"] = "LLM yanıtı oluşturulurken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Show), new { courseId, topicId, top });
        }
    }
}

