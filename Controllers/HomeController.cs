using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
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

        public HomeController(ILogger<HomeController> logger, TdnDbContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        // GET: /Home
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
        public async Task<IActionResult> Main(int? projectId = null, DaysViewName? daysViewName = null, string? openModal = null)
        {
            string? userId = _userManager.GetUserId(User);
            var defaultProject = GetOrCreateDefaultProject();
            projectId ??= defaultProject.ProjectId;

            // provide authorization for projectId

            TempData["CurrentProjectId"] = projectId; // => for select on the create task/note views

            string? dateOrder = TempData["DateOrder"] as string;
            TempData.Keep("DateOrder");
            string? hideCompletedString = TempData["HideCompleted"]?.ToString()?.ToLower(); // True => true
            Boolean.TryParse(hideCompletedString, out bool hideCompleted);

            if (dateOrder == null)
            {
                TempData["DateOrder"] = dateOrder = "descending";
            }
            if (hideCompleted == null)
            {
                TempData["HideCompleted"] = hideCompleted = false;
            }

            if (daysViewName == null)
            {
                ViewData["ReturnUrl"] = Url.Action(nameof(HomeController.Main), "Home", new { projectId });
            }
            else
            {
                ViewData["ReturnUrl"] = Url.Action(nameof(HomeController.Main), "Home", new { daysViewName });
            }

            if (daysViewName == DaysViewName.Today || daysViewName == DaysViewName.Upcoming)
            {
                TempData["DaysViewName"] = daysViewName;  // => title on the view
            }
            else if (daysViewName == DaysViewName.Unsorted || projectId != null)
            {
                TempData["DaysViewName"] = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId).Title;
            }

            GeneralViewModel generalViewModel = await LoadGeneralViewModel(daysViewName, userId, projectId);
            
            // sort by dateOrder, hideCompleted
            if (dateOrder == "ascending")
            {
                generalViewModel.Tasks = generalViewModel.Tasks.OrderBy(t => t.DueDate).ToList();
            }
            else
            {
                generalViewModel.Tasks = generalViewModel.Tasks.OrderByDescending(t => t.DueDate).ToList();
            }
            if (hideCompleted == true)
            {
                generalViewModel.Tasks = generalViewModel.Tasks.Where(t => t.IsCompleted == false).ToList();
            }

            return View(generalViewModel);
        }

        // GET: /Home/Labels
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
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Privacy
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // POST: /Home/ChangeTempDataValue
        [HttpPost]
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
        Models.Project GetOrCreateDefaultProject()
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
        async Task<GeneralViewModel> LoadGeneralViewModel(DaysViewName? daysViewName, string? userId, int? projectId)
        {
            GeneralViewModel generalViewModel = new GeneralViewModel();
            if (daysViewName == DaysViewName.Today)
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
                    .First();

                generalViewModel.Tasks.AddRange(currentProjectInclude.Tasks);
                generalViewModel.Notes.AddRange(currentProjectInclude.Notes);
            }
            generalViewModel.Labels = await _context.Labels.Where(l => l.UserId == userId).ToListAsync();
            return generalViewModel;
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
