using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            return PartialView("Tasks/_CreatePartial", new TaskLabelsViewModel()
            {
                Task = new Models.Task()
                {
                    ProjectId = currentProjectId
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
                if (taskLabels?.SelectedLabelsId != null)
                {
                    List<string>? selectedString = JsonSerializer.Deserialize<List<string>>(taskLabels?.SelectedLabelsId);
                    List<int>? selectedInt = selectedString?.Select(int.Parse).ToList();

                    var selected = _context.Labels.Where(l => selectedInt.Contains(l.LabelId.Value)).ToList();

                    taskLabels.Task.TaskLabels = new List<TaskLabel>();
                    foreach (var label in selected)
                    {
                        taskLabels.Task.TaskLabels.Add(new TaskLabel()
                        {
                            Label = label,
                        });
                    }
                }

                _context.Add(taskLabels.Task);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectTo = Url.Action(nameof(HomeController.Main), "Main") });
            }
            return PartialView("Tasks/_CreatePartial", taskLabels);
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
                    if (!TaskExists(label.LabelId))
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

        // GET: : Labels/Delete/5
        public async Task<IActionResult> DeletePartial(int? id)
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
            return PartialView("Labels/_DeletePartial", label);
        }

        // POST: Labels/ConfirmDelete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, string? returnUrl)
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
            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(HomeController.Main), "Home");
        }
        private bool TaskExists(int? id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
        private bool ProjectExists(int? id)
        {
            return _context.Projects.IgnoreQueryFilters().Any(e => e.ProjectId == id);
        }

    }
}
