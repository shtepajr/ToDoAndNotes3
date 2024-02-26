using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using ToDoAndNotes3.Authorization;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;

namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class LabelsController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public LabelsController(TdnDbContext context, UserManager<User> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Labels/MainListPartial
        [HttpGet]
        public IActionResult MainListPartial()
        {
            return PartialView("Labels/_MainListPartial", _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)));
        }

        // GET: Labels/CreatePartial
        [HttpGet]
        public IActionResult CreatePartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return PartialView("Labels/_CreatePartial", new Label());
        }

        // POST: Labels/CreatePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(Label label, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                label.UserId = _userManager.GetUserId(User);
                _context.Add(label);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = returnUrl });
            }
            return PartialView("Labels/_CreatePartial", label);
        }

        // GET: Labels/EditPartial/5
        [HttpGet]
        public async Task<IActionResult> EditPartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var label = await _context.Labels.FindAsync(id);

            if (label is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, label, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            return PartialView("Labels/_EditPartial", label);
        }

        // POST: Labels/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(Label label, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (label is null)
                {
                    return NotFound();
                }
                else
                {
                    var isAuthorized = await _authorizationService.AuthorizeAsync(User, label, EntityOperations.FullAccess);
                    if (!isAuthorized.Succeeded)
                    {
                        return Forbid();
                    }
                }
                try
                {
                    _context.Update(label);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabelExists(label.LabelId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { success = true, redirectTo = returnUrl });
            }

            return PartialView("Labels/_EditPartial", label);
        }

        // GET: : Labels/Delete/5
        [HttpGet]
        public async Task<IActionResult> DeletePartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var label = await _context.Labels.FindAsync(id);

            if (label is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, label, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            return PartialView("Labels/_DeletePartial", label);
        }

        // POST: Labels/ConfirmDelete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, string? returnUrl = null)
        {
            var label = await _context.Labels.FindAsync(id);

            if (label is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, label, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                _context.Labels.Remove(label);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, redirectTo = returnUrl });
        }

        #region Helpers       
        private bool LabelExists(int? id)
        {
            return _context.Labels.Any(e => e.LabelId == id);
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Labels), "Home");
            }
        }
        #endregion
    }
}
