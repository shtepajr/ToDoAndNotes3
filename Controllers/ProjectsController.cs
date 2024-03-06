using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ToDoAndNotes3.Authorization;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;
using ToDoAndNotes3.Models.MainViewModels;

namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public ProjectsController(TdnDbContext context, UserManager<User> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Projects/CreatePartial
        [HttpGet]
        public IActionResult CreatePartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return PartialView("Projects/_CreatePartial", new Models.Project());
        }

        // POST: Projects/CreatePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(Models.Project project, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                project.UserId = _userManager.GetUserId(User);
                _context.Add(project);
                await _context.SaveChangesAsync();
                returnUrl = Url.Action(nameof(HomeController.Main), "Home", new { projectId = project.ProjectId });
                return Json(new { success = true });
            }
            return PartialView("Projects/_CreatePartial", project);
        }

        // GET: Projects/EditPartial/5
        [HttpGet]
        public async Task<IActionResult> EditPartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var project = await _context.Projects.FindAsync(id);

            if (project is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            return PartialView("Projects/_EditPartial", project);
        }

        // POST: Projects/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(Models.Project project, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (project is null)
                {
                    return NotFound();
                }
                else
                {
                    var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                    if (!isAuthorized.Succeeded)
                    {
                        return Forbid();
                    }
                }
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
                return Json(new { success = true });
            }
            return PartialView("Projects/_EditPartial", project);
        }

        // POST: Projects/SoftDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int? id, string? returnUrl = null)
        {
            var project = _context.Projects
                .Include(t => t.Tasks)
                .Include(n => n.Notes)
                .SingleOrDefault(p => p.ProjectId == id);

            if (project is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                project.IsDeleted = true;
                foreach (var task in project?.Tasks)
                {
                    task.IsDeleted = true;
                }
                foreach (var note in project?.Notes)
                {
                    note.IsDeleted = true;
                }
                await _context.SaveChangesAsync();
                TempData["CurrentProjectId"] = null;
            }

            returnUrl = Url.Action(nameof(HomeController.Main), "Home", new { daysViewName = DaysViewName.Today });
            return RedirectToLocal(returnUrl);
        }

        // POST: Projects/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int? id, string? returnUrl = null)
        {
            var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                project.IsDeleted = false;

                var projectTasks = await _context.Tasks.IgnoreQueryFilters().Where(t => t.ProjectId == id).ToListAsync();
                var projectNotes = await _context.Tasks.IgnoreQueryFilters().Where(t => t.ProjectId == id).ToListAsync();

                foreach (var task in projectTasks)
                {
                    task.IsDeleted = false;
                }
                foreach (var note in projectNotes)
                {
                    note.IsDeleted = false;
                }
                await _context.SaveChangesAsync();
            }

            returnUrl = Url.Action(nameof(HomeController.Main), "Home", new { daysViewName = DaysViewName.Today });
            return RedirectToLocal(returnUrl);
        }

        // GET: Projects/DeletePartial/5
        [HttpGet]
        public async Task<IActionResult> DeletePartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            return PartialView("Projects/_DeletePartial", project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, string? returnUrl = null)
        {
            var project = await _context.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["CurrentProjectId"] = null;
            }

            return Json(new { success = true });
        }

        // POST: Projects/Duplicate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int? id, string? returnUrl = null)
        {
            var project = _context.Projects
                .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(tl => tl.Label)
                .Include(n => n.Notes).ThenInclude(n => n.NoteLabels).ThenInclude(nl => nl.Label)
                .Include(n => n.Notes).ThenInclude(n => n.NoteDescription)
                .FirstOrDefault(p => p.ProjectId == id);

            if (project is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                Models.Project? copy = DeepCopy(project);

                if (copy == null)
                {
                    return RedirectToLocal(returnUrl);
                }
                await _context.Projects.AddAsync(copy);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        #region Helpers  
        public static Models.Project? DeepCopy(Models.Project oldProject)
        {
            if (oldProject == null || oldProject.UserId == null)
            {
                return null;
            }

            Models.Project copy = new Models.Project()
            {
                UserId = oldProject.UserId,
                Title = "Copy of " + oldProject.Title
            };

            foreach (var task in oldProject?.Tasks)
            {
                var taskCopy = TasksController.DeepCopy(task);
                if (taskCopy != null)
                {
                    copy?.Tasks?.Add(taskCopy);
                }
            }
            foreach (var note in oldProject?.Notes)
            {
                var noteCopy = NotesController.DeepCopy(note);
                if (noteCopy != null)
                {
                    copy?.Notes?.Add(noteCopy);
                }
            }

            return copy;
        }
        private bool ProjectExists(int? id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Main), "Home", new { daysViewName = DaysViewName.Today });
            }
        }
        #endregion
    }
}
