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
    public class NotesController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public NotesController(TdnDbContext context, UserManager<User> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Notes/CreatePartial
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
                var project = await _context.Projects.FindAsync(noteLabels.Note.ProjectId);

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

                bool set = await SetSelectedLabelsAsync(noteLabels);
                if (!set)
                {
                    return NotFound();
                }

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
        [HttpGet]
        public async Task<IActionResult> EditPartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (id == null)
            {
                return NotFound();
            }

            var noteInclude = _context?.Notes
                ?.Where(n => n.NoteId == id)
                ?.Include(n => n.NoteDescription)
                ?.Include(n => n.NoteLabels)?.ThenInclude(nl => nl.Label)
                ?.Include(n => n.Project)
                ?.FirstOrDefault();

            if (noteInclude is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, noteInclude, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            IEnumerable<string?>? selectedInt = noteInclude?.NoteLabels?.Select(nl => nl?.Label?.LabelId.ToString());
            string selected = JsonSerializer.Serialize(selectedInt); // => "["12", "17"]"

            return PartialView("Notes/_EditPartial", new NoteLabelsViewModel()
            {
                Note = noteInclude!,
                SelectedLabelsId = selected,
                Labels = _context?.Labels?.Where(l => l.UserId == _userManager.GetUserId(User))?.ToList(),
                Projects = _context?.Projects?.Where(p => p.UserId == _userManager.GetUserId(User))?.ToList(),
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
                _context.Entry(noteLabels.Note).Reference(t => t.Project).Load(); // for next authorization
                _context.Attach(noteLabels.Note).State = EntityState.Modified;

                try
                {
                    bool set = await SetSelectedLabelsAsync(noteLabels);
                    if (!set)
                    {
                        return NotFound();
                    }
                    _context.Update(noteLabels.Note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(noteLabels.Note.NoteId))
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
            var note = await _context.Notes.Include(n => n.Project).FirstOrDefaultAsync(n => n.NoteId == id);

            if (note is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, note, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                note.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToLocal(returnUrl);
        }

        // GET: Notes/DeletePartial/5
        [HttpGet]
        public async Task<IActionResult> DeletePartial(int? id, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var note = await _context.Notes.Include(n => n.Project).FirstOrDefaultAsync(n => n.NoteId == id);

            if (note is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, note, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
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
            var note = await _context.Notes.Include(n => n.Project).FirstOrDefaultAsync(n => n.NoteId == id);

            if (note is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, note, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectTo = returnUrl });
        }

        // POST: Notes/Duplicate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int? id, string? returnUrl = null)
        {
            var note = await _context.Notes
                .Include(n => n.NoteDescription)
                .Include(n => n.NoteLabels).ThenInclude(nl => nl.Label)
                .Include(n => n.Project)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note is null)
            {
                return NotFound();
            }
            else
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, note, EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                Note? copy = DeepCopy(note);
                if (copy == null)
                {
                    return NotFound();
                }
                await _context.Notes.AddAsync(copy);
                await _context.SaveChangesAsync();
            }

            return RedirectToLocal(returnUrl);
        }
       
        #region Helpers
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
        private bool NoteExists(int? id)
        {
            return _context.Notes.Any(e => e.NoteId == id);
        }
        private async Task<bool> SetSelectedLabelsAsync(NoteLabelsViewModel noteLabelsViewModel)
        {
            if (noteLabelsViewModel?.SelectedLabelsId != null)
            {
                // SelectedLabelsId value: " \"12\" , \"17\" "
                List<string>? selectedString = JsonSerializer.Deserialize<List<string>>(noteLabelsViewModel?.SelectedLabelsId);
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

                if (noteLabelsViewModel.Note.NoteId is not null)
                {
                    var note = await _context?.Notes
                        ?.Include(n => n.Project)
                        ?.Include(n => n.NoteLabels)
                        ?.FirstOrDefaultAsync(n => n.NoteId == noteLabelsViewModel.Note.NoteId);

                    if (note is null)
                    {
                        return false;
                    }
                    else
                    {
                        var isAuthorized = await _authorizationService.AuthorizeAsync(User, note, EntityOperations.FullAccess);
                        if (!isAuthorized.Succeeded)
                        {
                            return false;
                        }
                    }
                    // if note is already exists then clear noteLabels (to recreate it)
                    //_context.Entry(noteLabelsViewModel.Note).Collection(t => t.NoteLabels).Load();
                    _context.RemoveRange(note.NoteLabels);
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
