using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

        public TasksController(TdnDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tasks/CreatePartial
        public IActionResult CreatePartial()
        {
            // provide authorization
            var currentProjectId = TempData["CurrentProjectId"] as int?;
            TempData.Keep("CurrentProjectId");

            DateOnly? defaultDate = null;
            // if days view
            if (Enum.TryParse(TempData["DaysViewName"]?.ToString(), out DaysViewName daysViewName))
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
        public async Task<IActionResult> CreatePartial(TaskLabelsViewModel taskLabels)
        {
            if (ModelState.IsValid)
            {
                if (!ProjectExists(taskLabels.Task.ProjectId))
                {
                    return NotFound();
                }

                // provide authorization
                SetSelectedLabels(taskLabels);

                _context.Add(taskLabels.Task);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = Url.Action(nameof(HomeController.Main), "Main") });
            }
            // get data for select lists (asp-for approach for each field uncomfortable on view)
            taskLabels.Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList();
            taskLabels.Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList();
            return PartialView("Tasks/_CreatePartial", taskLabels);
        }

        // GET: Tasks/EditPartial/5
        public async Task<IActionResult> EditPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskInclude = _context.Tasks
                .Where(t => t.TaskId == id)
                .Include(t => t.TaskLabels)?.ThenInclude(t => t.Label)
                .FirstOrDefault();

            if (taskInclude == null)
            {
                return NotFound();
            }

            IEnumerable<int?> selectedInt = taskInclude?.TaskLabels?.Select(tl => tl.Label.LabelId);
            string selected = JsonSerializer.Serialize(selectedInt);

            return PartialView("Tasks/_EditPartial", new TaskLabelsViewModel()
            {
                Task = taskInclude!,
                SelectedLabelsId = selected,
                Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList(),
                Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList(),
            });
        }

        // POST: Tasks/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(TaskLabelsViewModel taskLabels)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SetSelectedLabels(taskLabels);
                    _context.Update(taskLabels.Task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(taskLabels.Task.TaskId))
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
            // get data for select lists (asp-for approach for each field uncomfortable on view)
            taskLabels.Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList();
            taskLabels.Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList();
            return PartialView("Tasks/_EditPartial", taskLabels);
        }

        // POST: Tasks/SoftDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int? id)
        {
            var task = _context.Tasks.SingleOrDefault(p => p.TaskId == id);

            if (task != null)
            {
                task.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(HomeController.Main), "Home");
        }
       
        // GET: Tasks/DeletePartial/5
        public async Task<IActionResult> DeletePartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
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
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectTo = Url.Action(nameof(HomeController.Main), "Home") });
        }

        // POST: Tasks/Duplicate/5
        public async Task<IActionResult> Duplicate(int? id)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskLabels)
                .ThenInclude(tl => tl.Label)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task != null)
            {
                Models.Task? copy = TasksController.DeepCopy(task);
                if (copy == null)
                {
                    return RedirectToAction(nameof(HomeController.Main), "Home");
                }
                await _context.Tasks.AddAsync(copy);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(HomeController.Main), "Home");
        }
    
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
        private bool ProjectExists(int? id)
        {
            return _context.Projects.IgnoreQueryFilters().Any(e => e.ProjectId == id);
        }
        private void SetSelectedLabels(TaskLabelsViewModel taskLabelsViewModel)
        {
            if (taskLabelsViewModel?.SelectedLabelsId != null)
            {
                List<string>? selectedString = JsonSerializer.Deserialize<List<string>>(taskLabelsViewModel?.SelectedLabelsId);
                List<int>? selectedInt = selectedString?.Select(int.Parse).ToList();

                var selected = _context.Labels.Where(l => selectedInt.Contains(l.LabelId.Value)).ToList();

                if (TaskExists(taskLabelsViewModel.Task.TaskId))
                {
                    _context.Entry(taskLabelsViewModel.Task).Collection(t => t.TaskLabels).Load();
                    _context.RemoveRange(taskLabelsViewModel.Task.TaskLabels);
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
            }
        }
    }
}
