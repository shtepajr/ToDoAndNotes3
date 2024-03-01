using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ToDoAndNotes3.Authorization;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;
using ToDoAndNotes3.Models.MainViewModels;


namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public TasksController(TdnDbContext context, UserManager<User> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Tasks/CreatePartial
        [HttpGet]
        public async Task<IActionResult> CreatePartialAsync(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var currentProjectId = TempData.Peek("CurrentProjectId") as int?;
            var project = await _context.Projects.FindAsync(currentProjectId);

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

            DateOnly? defaultDate = null;
            // if days view
            if (Enum.TryParse(TempData.Peek("DaysViewName")?.ToString(), out DaysViewName daysViewName))
            {
                defaultDate = DateOnly.FromDateTime(DateTime.Now);
            }
            TempData.Keep("DaysViewName");

            return PartialView("Tasks/_CreatePartial", new TaskLabelsViewModel()
            {
                Task = new Models.Task()
                {
                    ProjectId = currentProjectId,
                    DueDate = defaultDate,
                },
                Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList(),
                Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList(),
            });
        }

        // POST: Tasks/CreatePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(TaskLabelsViewModel taskLabels, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var project = await _context.Projects.FindAsync(taskLabels.Task.ProjectId);

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

                bool set = await SetSelectedLabelsAsync(taskLabels);
                if (!set)
                {
                    return NotFound();
                }

                _context.Add(taskLabels.Task);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = returnUrl });
            }
            // get data for select lists (asp-for approach for each field uncomfortable on view)
            taskLabels.Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList();
            taskLabels.Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList();
            return PartialView("Tasks/_CreatePartial", taskLabels);
        }

        // GET: Tasks/EditPartial/5
        [HttpGet]
        public async Task<IActionResult> EditPartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (id == null)
            {
                return NotFound();
            }

            var taskInclude = _context?.Tasks
                ?.Where(t => t.TaskId == id)
                ?.Include(t => t.TaskLabels)?.ThenInclude(t => t.Label)
                ?.Include(t => t.Project)
                .FirstOrDefault();

            if (taskInclude is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, taskInclude, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            IEnumerable<string?>? selectedInt = taskInclude?.TaskLabels?.Select(tl => tl?.Label?.LabelId.ToString());
            string selected = JsonSerializer.Serialize(selectedInt); // => "["12", "17"]"

            var test = new TaskLabelsViewModel()
            {
                Task = taskInclude!,
                SelectedLabelsId = selected,
                Labels = _context?.Labels?.Where(l => l.UserId == _userManager.GetUserId(User))?.ToList(),
                Projects = _context?.Projects?.Where(p => p.UserId == _userManager.GetUserId(User))?.ToList(),
            };
            foreach (var item in test.Projects)
            {
                Console.WriteLine("----------------");
                Console.WriteLine(item.ProjectId);
                Console.WriteLine(item.Title);
            }

            return PartialView("Tasks/_EditPartial", test);
        }

        // POST: Tasks/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(TaskLabelsViewModel taskLabels, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                _context.Entry(taskLabels.Task).Reference(t => t.Project).Load(); // for next authorization
                _context.Attach(taskLabels.Task).State = EntityState.Modified;

                try
                {
                    bool set = await SetSelectedLabelsAsync(taskLabels);
                    if (!set)
                    {
                        return NotFound();
                    }
                    _context.Update(taskLabels.Task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(taskLabels.Task.TaskId))
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
            // get data for select lists (asp-for approach for each field uncomfortable on view)
            taskLabels.Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList();
            taskLabels.Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList();
            return PartialView("Tasks/_EditPartial", taskLabels);
        }

        // POST: Tasks/SoftDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int? id, string? returnUrl = null)
        {
            var task = await _context.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                task.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // POST: Tasks/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int? id, string? returnUrl = null)
        {
            var task = await _context.Tasks.Include(t => t.Project).IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                task.IsDeleted = false;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // GET: Tasks/DeletePartial/5
        [HttpGet]
        public async Task<IActionResult> DeletePartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var task = await _context.Tasks.Include(t => t.Project).IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            return PartialView("Tasks/_DeletePartial", new TaskLabelsViewModel()
            {
                Task = task
            });
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, string? returnUrl = null)
        {
            var task = await _context.Tasks.Include(t => t.Project).IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // POST: Tasks/Duplicate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int? id, string? returnUrl = null)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                Models.Task? copy = DeepCopy(task);
                if (copy == null)
                {
                    return NotFound();
                }
                await _context.Tasks.AddAsync(copy);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // POST: Tasks/ToggleState/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleState(int? id, string? returnUrl = null)
        {
            var task = await _context.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                task.IsCompleted = !task.IsCompleted;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, redirectTo = returnUrl });
        }

        #region Helpers
        public static Models.Task? DeepCopy(Models.Task oldTask)
        {
            if (oldTask == null || oldTask.ProjectId == null)
            {
                return null;
            }

            Models.Task copy = new Models.Task()
            {
                ProjectId = oldTask.ProjectId,
                Title = oldTask.Title,
                Description = oldTask.Description,
                DueDate = oldTask.DueDate,
                DueTime = oldTask.DueTime,
                TaskLabels = new List<TaskLabel>()
            };

            foreach (var old in oldTask?.TaskLabels)
            {
                copy?.TaskLabels?.Add(new TaskLabel()
                {
                    Task = copy,
                    Label = old.Label,
                });
            }
            return copy;
        }
        private bool TaskExists(int? id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
        private async Task<bool> SetSelectedLabelsAsync(TaskLabelsViewModel taskLabelsViewModel)
        {
            if (taskLabelsViewModel?.SelectedLabelsId != null)
            {
                // SelectedLabelsId value: "["17","27"]" ~ "[\"17\",\"27\"]"
                List<string>? selectedString = JsonSerializer.Deserialize<List<string>>(taskLabelsViewModel?.SelectedLabelsId);
                List<int>? selectedInt = selectedString?.Select(int.Parse).ToList();

                var selected = _context.Labels.Where(l => selectedInt.Contains(l.LabelId.Value)).ToList();

                foreach (var label in selected)
                {
                    if (label is null)
                    {
                        return false;
                    }
                    else
                    {
                        var isAuthorized = await _authorizationService.AuthorizeAsync(User, label, EntityOperations.FullAccess);
                        if (!isAuthorized.Succeeded)
                        {
                            return false;
                        }
                    }
                }

                if (taskLabelsViewModel.Task.TaskId is not null)
                {
                    var task = await _context?.Tasks
                        ?.Include(n => n.Project)
                        ?.Include(n => n.TaskLabels)
                        ?.FirstOrDefaultAsync(t => t.TaskId == taskLabelsViewModel.Task.TaskId);

                    if (task is null)
                    {
                        return false;
                    }
                    else
                    {
                        var isAuthorized = await _authorizationService.AuthorizeAsync(User, task, EntityOperations.FullAccess);
                        if (!isAuthorized.Succeeded)
                        {
                            return false;
                        }
                    }
                    // if task is already exists then clear noteLabels (to recreate it)
                    //_context.Entry(taskLabelsViewModel.Note).Collection(t => t.NoteLabels).Load();
                    _context.RemoveRange(task.TaskLabels);
                }

                List<TaskLabel> taskLabels = new List<TaskLabel>();
                foreach (var label in selected)
                {
                    taskLabels.Add(new TaskLabel()
                    {
                        Label = label
                    });
                }
                taskLabelsViewModel.Task.TaskLabels = taskLabels;
                return true;
            }
            return true;
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
