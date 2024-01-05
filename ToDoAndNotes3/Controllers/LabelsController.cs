using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;

namespace ToDoAndNotes3.Controllers
{
    public class LabelsController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;

        public LabelsController(TdnDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Labels/CreatePartial
        public IActionResult CreatePartial()
        {
            return PartialView("Labels/_CreatePartial", new Label());
        }

        // POST: Labels/CreatePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(Label label)
        {
            if (ModelState.IsValid)
            {
                label.UserId = _userManager.GetUserId(User);
                _context.Add(label);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = Url.Action(nameof(HomeController.Main), "Labels") });
            }
            return PartialView("Labels/_CreatePartial", label);
        }

        // GET: Labels/EditPartial/5
        public async Task<IActionResult> EditPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var label = await _context.Labels.FindAsync(id);
            if (label == null)
            {
                return NotFound();
            }
            return PartialView("Labels/_EditPartial", label);
        }

        // POST: Labels/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(Label label)
        {
            if (ModelState.IsValid)
            {
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
                return Json(new { success = true, redirectTo = Url.Action(nameof(HomeController.Main), "Home") });
            }
            return PartialView("Labels/_EditPartial", label);
        }


        private bool LabelExists(int? id)
        {
            return _context.Labels.Any(e => e.LabelId == id);
        }
    }
}
