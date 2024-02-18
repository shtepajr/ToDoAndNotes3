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

        // GET: Notes/EditPartial/5
        public async Task<IActionResult> EditPartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (id == null)
            {
                return NotFound();
            }

            var noteInclude = _context.Notes
                .Where(n => n.NoteId == id)
                .Include(n => n.NoteDescription)
                .Include(n => n.NoteLabels)?.ThenInclude(nl => nl.Label)
                .FirstOrDefault();

            if (noteInclude == null)
            {
                return NotFound();
            }

            IEnumerable<int?> selectedInt = noteInclude?.NoteLabels?.Select(nl => nl.Label.LabelId);
            string selected = JsonSerializer.Serialize(selectedInt); // => "[12, 17]"

            return PartialView("Notes/_EditPartial", new NoteLabelsViewModel()
            {
                Note = noteInclude!,
                SelectedLabelsId = selected,
                Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList(),
                Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList(),
            });
        }

        // POST: Notes/EditPartial/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartial(NoteLabelsViewModel noteLabels, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    SetSelectedLabels(noteLabels);
                    _context.Update(noteLabels.Note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(noteLabels.Note.NoteId))
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
            noteLabels.Labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User)).ToList();
            noteLabels.Projects = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User)).ToList();
            return PartialView("Notes/_EditPartial", noteLabels);
        }

        // POST: Notes/SoftDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int? id, string? returnUrl = null)
        {
            var note = _context.Notes.SingleOrDefault(n => n.NoteId == id);

            if (note != null)
            {
                note.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToLocal(returnUrl);
        }
       
        // GET: Notes/DeletePartial/5
        public async Task<IActionResult> DeletePartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return PartialView("Notes/_DeletePartial", new NoteLabelsViewModel()
            {
                Note = note
            });
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, string? returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // POST: Notes/Duplicate/5
        public async Task<IActionResult> Duplicate(int? id, string? returnUrl = null)
        {
            var note = await _context.Notes
                .Include(n => n.NoteDescription)
                .Include(n => n.NoteLabels).ThenInclude(nl => nl.Label)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note != null)
            {
                Note? copy = NotesController.DeepCopy(note);
                if (copy == null)
                {
                    return RedirectToLocal(returnUrl);
                }
                await _context.Notes.AddAsync(copy);
                await _context.SaveChangesAsync();
            }
            return RedirectToLocal(returnUrl);
        }
       
        public static Note? DeepCopy(Note oldNote)
        {
            if (oldNote == null || oldNote.ProjectId == null)
            {
                return null;
            }

            Note copy = new Note()
            {
                ProjectId = oldNote.ProjectId,
                Title = oldNote.Title,
                ShortDescription = oldNote.ShortDescription,
                NoteDescription = new NoteDescription()
                {
                    Description = oldNote.NoteDescription.Description,
                },
                DueDate = oldNote.DueDate,
                DueTime = oldNote.DueTime,
                NoteLabels = new List<NoteLabel>()
            };

            foreach (var old in oldNote?.NoteLabels)
            {
                copy?.NoteLabels?.Add(new NoteLabel()
                {
                    Note = copy,
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
