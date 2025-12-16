using Microsoft.AspNetCore.Mvc;
using LearnPrompt.Web.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace LearnPrompt.Web.Controllers;

[AllowAnonymous]
public class IdentityController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public IdentityController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded) return LocalRedirect(model.ReturnUrl ?? "/");

        ModelState.AddModelError(string.Empty,
            result.IsLockedOut ? "Hesabınız geçici olarak kilitlendi." : "Geçersiz giriş denemesi.");

        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new RegisterViewModel
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
       
        var user = new IdentityUser
        {
            UserName =  model.Email.Split("@")[0],  
            Email = model.Email
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);

        if (createResult.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(model.ReturnUrl ?? "/");
        }

        foreach (var err in createResult.Errors)
            ModelState.AddModelError(string.Empty, err.Description);

        return View(model);
    }

    [HttpGet] public IActionResult AccessDenied() => View();
    [HttpGet] public IActionResult NotFound() => View();
    [HttpGet] public IActionResult Error() => View();
}