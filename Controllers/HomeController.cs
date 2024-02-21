using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using ToDoAndNotes3.Authorization;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;
using ToDoAndNotes3.Models.MainViewModels;

namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public HomeController(ILogger<HomeController> logger, TdnDbContext context, UserManager<User> userManager, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }
        // GET: /Home
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Main), new { daysViewName = DaysViewName.Today });
            }
            return View();
        }

        // GET: /Home/Main
        [HttpGet]
        public async Task<IActionResult> Main(int? projectId = null, DaysViewName? daysViewName = null, string? openModal = null, 
            string? search = null, int? labelId = null)
        {
            string? userId = _userManager.GetUserId(User);
            var defaultProject = GetOrCreateDefaultProject();
            
            if (projectId is not null)
            {
       
                var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                  User, _context.Projects.Find(projectId), EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }
            if (labelId is not null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                  User, _context.Labels.Find(labelId), EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
            }

            projectId ??= defaultProject.ProjectId;
            TempData["CurrentProjectId"] = projectId; // => for select on the create task/note views

            string? dateOrder = TempData.Peek("DateOrder") as string;
            string? hideCompletedString = TempData["HideCompleted"]?.ToString()?.ToLower(); // True => true
            Boolean.TryParse(hideCompletedString, out bool hideCompleted);

            if (daysViewName == null)
            {
                ViewData["ReturnUrl"] = Url.Action(nameof(HomeController.Main), "Home", new { projectId });
            }
            else
            {
                ViewData["ReturnUrl"] = Url.Action(nameof(HomeController.Main), "Home", new { daysViewName });
            }

            // title of "data container"
            if (labelId is not null)
            {
                ViewData["DisplayDataTitle"] = "Label: " + _context.Labels.Find(labelId).Title;
            }
            else if (search is not null)
            {
                ViewData["Search"] = search;
                ViewData["ReturnUrl"] += $"&search={search}"; // next sorts won't break search
                ViewData["DisplayDataTitle"] = "Search: " + search;
            }
            else if (daysViewName == DaysViewName.Today || daysViewName == DaysViewName.Upcoming)
            {
                TempData["DaysViewName"] = daysViewName; // for Create Tasks/Notes to understand default date
                ViewData["DisplayDataTitle"] = daysViewName;
            }
            else if (daysViewName == DaysViewName.Unsorted || projectId != null)
            {
                ViewData["DisplayDataTitle"] = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId).Title;
            }

            // sort things
            if (dateOrder == null)
            {
                TempData["DateOrder"] = dateOrder = "descending";
            }
            if (hideCompleted == null)
            {
                TempData["HideCompleted"] = hideCompleted = false;
            }

            // check data loading time
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            GeneralViewModel generalViewModel = await LoadGeneralViewModel(daysViewName, userId, projectId, labelId, search);

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            Console.WriteLine("Elapsed time: " + elapsedTime);

            SortGeneralViewModel(generalViewModel, dateOrder, hideCompleted);

            return View(generalViewModel);
        }

        // GET: /Home/Labels
        [HttpGet]
        public async Task<IActionResult> Labels()
        {
            ViewData["ReturnUrl"] = Url.Action(nameof(HomeController.Labels), "Home");

            ProjectLabelViewModel projectLabelViewModel = new ProjectLabelViewModel();
            string? userId = _userManager.GetUserId(User);
            var projects = await _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToListAsync();
            var labels = await _context.Labels.Where(p => p.UserId == userId).ToListAsync();
            projectLabelViewModel.Projects = projects;
            projectLabelViewModel.Labels = labels;
            return View(projectLabelViewModel);
        }

        // GET: /Home/Privacy
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // POST: /Home/ChangeTempDataValue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeTempDataValue(string? tempDataName, string? tempDataValue = null, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(tempDataName))
            {
                return Json(new { success = false });
            }
            TempData[tempDataName] = tempDataValue;
            return Json(new { success = true, redirectTo = returnUrl });
        }

        #region Helpers
        private Models.Project GetOrCreateDefaultProject()
        {
            var checkDefault = _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User) 
                                                                && p.IsDefault == true).FirstOrDefault();
            if (checkDefault == null)
            {
                var defaultProject = new Models.Project()
                {
                    IsDefault = true,
                    Title = "Unsorted",
                    UserId = _userManager.GetUserId(User),
                };
                _context.Add(defaultProject);
                _context.SaveChanges();
                return defaultProject;
            }
            return checkDefault;
        }
        private async Task<GeneralViewModel> LoadGeneralViewModel(DaysViewName? daysViewName, string? userId, int? projectId, int? labelId, string? search)
        {
            GeneralViewModel generalViewModel = new GeneralViewModel();
            if (labelId is not null)
            {
                var projectsLabelInclude = await _context.Projects
                    .Where(p => p.UserId == userId)
                    .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes).ThenInclude(t => t.NoteLabels).ThenInclude(l => l.Label)
                    .ToListAsync();
                generalViewModel.Projects = projectsLabelInclude.Where(p => p.IsDefault == false).ToList(); // do not show default project

                foreach (var project in projectsLabelInclude) // but here using default project
                {
                    generalViewModel.Tasks.AddRange(project.Tasks.Where(t => t.TaskLabels.Any(tl => tl.Label.LabelId == labelId)));
                    generalViewModel.Notes.AddRange(project.Notes.Where(n => n.NoteLabels.Any(nl => nl.Label.LabelId == labelId)));
                }
            }
            else if (search is not null)
            {
                var projectsLabelInclude = await _context.Projects
                    .Where(p => p.UserId == userId)
                    .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes).ThenInclude(t => t.NoteLabels).ThenInclude(l => l.Label)
                    .ToListAsync();
                generalViewModel.Projects = projectsLabelInclude.Where(p => p.IsDefault == false).ToList(); // do not show default project

                foreach (var project in projectsLabelInclude) // but here using default project
                {
                    generalViewModel.Tasks.AddRange(project.Tasks.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase)));
                    generalViewModel.Notes.AddRange(project.Notes.Where(n => n.Title.Contains(search, StringComparison.OrdinalIgnoreCase)));
                }
            }
            else if (daysViewName == DaysViewName.Today)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);

                var projectsTodayInclude = await _context.Projects
                    .Where(p => p.UserId == userId)
                    .Include(t => t.Tasks.Where(t => t.DueDate == today)).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes.Where(n => n.DueDate == today)).ThenInclude(n => n.NoteLabels).ThenInclude(l => l.Label)
                    .ToListAsync();
                generalViewModel.Projects = projectsTodayInclude.Where(p => p.IsDefault == false).ToList();

                foreach (var project in projectsTodayInclude)
                {
                    generalViewModel.Tasks.AddRange(project.Tasks);
                    generalViewModel.Notes.AddRange(project.Notes);
                }
            }
            else if (daysViewName == DaysViewName.Upcoming)
            {
                var projectsUpcomingInclude = await _context.Projects
                    .Where(p => p.UserId == userId)
                    .Include(t => t.Tasks.Where(t => t.DueDate != null)).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes.Where(n => n.DueDate != null)).ThenInclude(t => t.NoteLabels).ThenInclude(l => l.Label)
                    .ToListAsync();
                generalViewModel.Projects = projectsUpcomingInclude.Where(p => p.IsDefault == false).ToList(); // do not show default project

                foreach (var project in projectsUpcomingInclude) // but here using default project
                {
                    generalViewModel.Tasks.AddRange(project.Tasks);
                    generalViewModel.Notes.AddRange(project.Notes);
                }
            }
            else if (daysViewName == DaysViewName.Unsorted || projectId != null)
            {
                // 1/2 load projects list separately to save resources
                generalViewModel.Projects = await _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToListAsync();
                // 2/2 load current project content separately to save resources
                var currentProjectInclude = _context.Projects
                    .Where(p => p.UserId == userId && p.ProjectId == projectId)
                    .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes).ThenInclude(n => n.NoteLabels).ThenInclude(l => l.Label)
                    .FirstOrDefault();

                generalViewModel.Tasks.AddRange(currentProjectInclude.Tasks);
                generalViewModel.Notes.AddRange(currentProjectInclude.Notes);
            }
            
            generalViewModel.Labels = await _context.Labels.Where(l => l.UserId == userId).ToListAsync();
            return generalViewModel;
        }
        private void SortGeneralViewModel(GeneralViewModel generalViewModel, string? dateOrder, bool? hideCompleted)
        {
            // sort by dateOrder, hideCompleted
            if (dateOrder == "ascending")
            {
                generalViewModel.Tasks = generalViewModel.Tasks.OrderBy(t => t.DueDate).ThenBy(t => t.DueTime).ToList();
                generalViewModel.Notes = generalViewModel.Notes.OrderBy(t => t.DueDate).ThenBy(t => t.DueTime).ToList();
            }
            else
            {
                generalViewModel.Tasks = generalViewModel.Tasks.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.DueTime).ToList();
                generalViewModel.Notes = generalViewModel.Notes.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.DueTime).ToList();
            }
            if (hideCompleted == true)
            {
                generalViewModel.Tasks = generalViewModel.Tasks.Where(t => t.IsCompleted == false).ToList();
            }
            else
            {
                generalViewModel.Tasks = generalViewModel.Tasks.OrderBy(t => t.IsCompleted).ToList();
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

        // test
        void SeedDbData()
        {
            if (_context.Projects.Any() || _context.Labels.Any())
            {
                return;
                //_context.Projects.RemoveRange(_context.Projects);
                //_context.Labels.RemoveRange(_context.Labels);
            }
            //_context.Database.EnsureDeleted();
            //_context.Database.EnsureCreated();

            for (int i = 0; i < 5; i++)
            {
                _context.Labels.Add(new Label()
                {
                    UserId = _userManager.GetUserId(User),
                    Title = "Label title lorem" + i,
                });
            }

            for (int i = 0; i < 10; i++)
            {
                _context.Projects.Add(new Models.Project()
                {
                    UserId = _userManager.GetUserId(User),
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    Title = "Project title lorem" + i,
                    Tasks = new List<Models.Task>()
                    {
                        new Models.Task()
                        {
                            Title = "Task title lorem " + i,
                            Description = "Desc",
                        }
                    }                   
                });
            }            
            _context.SaveChanges();
        }
        #endregion
    }
}
