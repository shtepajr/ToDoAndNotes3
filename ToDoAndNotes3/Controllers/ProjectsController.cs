using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;

namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectsController(TdnDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return PartialView("Projects/_IndexPartial", await _context.Projects.ToListAsync());
        }

        // GET: Projects/CreatePartial
        public IActionResult CreatePartial()
        {
            return PartialView("Projects/_CreatePartial", new Project());
        }

        // POST: Projects/CreatePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(Project project)
        {
            if (ModelState.IsValid)
            {
                project.UserId = _userManager.GetUserId(User);
                _context.Add(project);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = Url.Action(nameof(HomeController.Main), "Home") });
            }
            return PartialView("Projects/_CreatePartial", project);
        }

        // GET: Projects/EditPartial/5
        public async Task<IActionResult> EditPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return PartialView("Projects/_EditPartial", project);
        }

        // POST: Projects/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(Project project)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId))
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
            return PartialView("Projects/_EditPartial", project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/SoftDelete/5
        public async Task<IActionResult> SoftDelete(int? id)
        {
            var project = _context.Projects
                .Include(t => t.Tasks)
                .Include(n => n.Notes)
                .SingleOrDefault(p => p.ProjectId == id);
            if (project != null)
            {
                project.IsDeleted = true;
                foreach (var task in project.Tasks)
                {
                    task.IsDeleted = true;
                }
                foreach (var note in project.Notes)
                {
                    note.IsDeleted = true;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(HomeController.Main), "Home");
        }
        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(HomeController.Main), "Home");
        }

        public async Task<IActionResult> Duplicate(int? id)
        {
            var project = _context.Projects
                .Include(t => t.Tasks)
                .Include(n => n.Notes)
                .SingleOrDefault(p => p.ProjectId == id);
            if (project != null)
            {
                Project copy = new Project()
                {
                    UserId = project.UserId,
                    Title = "Copy of " +  project.Title                   
                };
                
                foreach (var task in project.Tasks)
                {
                    copy.Tasks.Add(new Models.Task()
                    {
                        Description = task.Description,
                        DueDate = task.DueDate,
                        Title = task.Title,
                        //TaskLabels = task.TaskLabels
                    });
                }
                foreach (var note in project.Notes)
                {
                    copy.Notes.Add(new Note()
                    {
                        Description = note.Description,
                        DueDate = note.DueDate,
                        Title = note.Title,
                        //TaskLabels = task.TaskLabels
                    });
                }

                await _context.Projects.AddAsync(copy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(HomeController.Main), "Home");
            }
            // mb handle error somehow
            return RedirectToAction(nameof(HomeController.Main), "Home");
        }
        private bool ProjectExists(int? id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
