using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using ToDoAndNotes3.Models;
using ToDoAndNotes3.Models.ManageViewModels;
using ToDoAndNotes3.Data;
using Microsoft.EntityFrameworkCore;

namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly TdnDbContext _context;

        public ManageController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ILoggerFactory loggerFactory,
            TdnDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = loggerFactory.CreateLogger<ManageController>();
            _context = context;
        }


        // GET: Manage/ChangeNamePartial
        [HttpGet]
        public async Task<IActionResult> ChangeNamePartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user = await _userManager.GetUserAsync(User);

            return PartialView("Manage/_ChangeNamePartial", new ChangeNameViewModel()
            {
                OldName = user.CustomName
            });
        }

        // POST: Manage/ChangeNamePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeNamePartial(ChangeNameViewModel changeName, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.CustomName = changeName.NewName;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User changed their name successfully.");
                        return Json(new { success = true, message= "Name changed successfully" });
                    }
                    AddErrors(result);
                }
            }
            return PartialView("Manage/_ChangeNamePartial", changeName);
        }

        // GET: Manage/ChangeNamePartial
        [HttpGet]
        public async Task<IActionResult> ChangeEmailPartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user = await _userManager.GetUserAsync(User);

            return PartialView("Manage/_ChangeEmailPartial", new ChangeEmailViewModel()
            {
                OldEmail = user.Email
            });
        }

        // POST: Manage/ChangeNamePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmailPartial(ChangeEmailViewModel changeEmail, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var email = await _userManager.GetEmailAsync(user);
                if (changeEmail.NewEmail != email)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmail.NewEmail);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action(
                        nameof(AccountController.ChangeEmailConfirm), "Account", 
                        new { userId = userId, email=changeEmail.NewEmail,  code = code }, 
                        protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(
                        changeEmail.NewEmail,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    returnUrl = Url.Action(nameof(AccountController.CheckEmail), "Account", new { email = changeEmail.NewEmail });
                    return Json(new { success = true, redirectTo = returnUrl });
                }
            }
            return PartialView("Manage/_ChangeEmailPartial", changeEmail);
        }

        // GET: /Manage/ChangePasswordPartial
        [HttpGet]
        public IActionResult ChangePasswordPartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return PartialView("Manage/_ChangePasswordPartial", new ChangePasswordViewModel());
        }

        // POST: /Manage/ChangePasswordPartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordPartial(ChangePasswordViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User changed their password successfully.");
                        return Json(new { success = true, message = "Password changed successfully" });
                    }
                    AddErrors(result);
                }
            }
            return PartialView("Manage/_ChangePasswordPartial", model);
        }

        // GET: /Manage/SetPasswordPartial
        [HttpGet]
        public IActionResult SetPasswordPartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return PartialView("Manage/_SetPasswordPartial", new SetPasswordViewModel());
        }

        // POST: /Manage/SetPasswordPartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPasswordPartial(SetPasswordViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent:true);
                        _logger.LogInformation("User set their password successfully.");
                        return Json(new { success = true, message = "Password set successfully" });
                    }
                    AddErrors(result);
                }
            }
            return PartialView("Manage/_SetPasswordPartial", model);
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLoginPartial(RemoveLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: true);
                        _logger.LogInformation("User removed their login successfully.");
                        return Json(new { success = true, message = "Login removed successfully" });
                    }
                    AddErrors(result);
                }
            }
            return PartialView("Manage/_RemoveLoginPartial", model);
        }

        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }
        
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToAction(nameof(HomeController.Manage), "Home");
            }
            var result = await _userManager.AddLoginAsync(user, info);

            if (result.Succeeded)
            {
                _logger.LogInformation("User added their login successfully.");
            }
            else
            {
                _logger.LogError("Failed to add user login.");
            }
            return RedirectToAction(nameof(HomeController.Manage), "Home");
        }
        
        // GET: /Manage/DeleteAccount
        [HttpGet]
        public IActionResult DeleteAccountPartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return PartialView("Manage/_DeleteAccountPartial");
        }

        // POST: /Manage/DeleteAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // delete projects and labels, then user
            _context.Projects.RemoveRange(_context.Projects.IgnoreQueryFilters().Where(p => p.UserId == user.Id));
            _context.Labels.RemoveRange(_context.Labels.Where(l => l.UserId == user.Id));
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);
                TempData.Clear();
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction(nameof(AccountController.Register), "Account");
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<User> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
