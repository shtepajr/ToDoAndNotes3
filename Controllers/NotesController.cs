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
    public class NotesController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;

        public NotesController(TdnDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Notes/CreatePartial
        public IActionResult CreatePartial(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

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

            return PartialView("Notes/_CreatePartial", new NoteLabelsViewModel()
            {
                Note = new Note()
                {
                    ProjectId = currentProjectId,
                    DueDate = defaultDate,
                },
                Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList(),
                Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList(),
            });
        }

        // POST: Notes/CreatePartial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(NoteLabelsViewModel noteLabels, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (!ProjectExists(noteLabels.Note.ProjectId))
                {
                    return NotFound();
                }

                // provide authorization
                SetSelectedLabels(noteLabels);

                _context.Add(noteLabels.Note);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = returnUrl });
            }
            // get data for select lists (asp-for approach for each field uncomfortable on view)
            noteLabels.Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList();
            noteLabels.Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList();
            return PartialView("Notes/_CreatePartial", noteLabels);
        }

        // GET: Tasks/EditPartial/5
        public async Task<IActionResult> EditPartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

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
            string selected = JsonSerializer.Serialize(selectedInt); // => "[12, 17]"

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
        public async Task<IActionResult> EditPartial(TaskLabelsViewModel taskLabels, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    //SetSelectedLabels(taskLabels);
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
            var task = _context.Tasks.SingleOrDefault(p => p.TaskId == id);

            if (task != null)
            {
                task.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToLocal(returnUrl);
        }
       
        // GET: Tasks/DeletePartial/5
        public async Task<IActionResult> DeletePartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

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

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // POST: Tasks/Duplicate/5
        public async Task<IActionResult> Duplicate(int? id, string? returnUrl = null)
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
                    return RedirectToLocal(returnUrl);
                }
                await _context.Tasks.AddAsync(copy);
                await _context.SaveChangesAsync();
            }
            return RedirectToLocal(returnUrl);
        }

        // POST: Tasks/ToggleState/5
        public async Task<IActionResult> ToggleState(int? id, string? returnUrl = null)
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

            task.IsCompleted = !task.IsCompleted;
            await _context.SaveChangesAsync();

            return RedirectToLocal(returnUrl);
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
        private void SetSelectedLabels(NoteLabelsViewModel noteLabelsViewModel)
        {
            if (noteLabelsViewModel?.SelectedLabelsId != null)
            {
                // SelectedLabelsId value: " \"12\" , \"17\" "
                List<string>? selectedString = JsonSerializer.Deserialize<List<string>>(noteLabelsViewModel?.SelectedLabelsId);
                List<int>? selectedInt = selectedString?.Select(int.Parse).ToList();

                var selected = _context.Labels.Where(l => selectedInt.Contains(l.LabelId.Value)).ToList();

                if (TaskExists(noteLabelsViewModel.Note.NoteId))
                {
                    _context.Entry(noteLabelsViewModel.Note).Collection(t => t.NoteLabels).Load();
                    _context.RemoveRange(noteLabelsViewModel.Note.NoteLabels);
                }

                List<NoteLabel> noteLabels = new List<NoteLabel>();
                foreach (var label in selected)
                {
                    noteLabels.Add(new NoteLabel()
                    {
                        Label = label
                    });
                }
                noteLabelsViewModel.Note.NoteLabels = noteLabels;
            }
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
    }
}
