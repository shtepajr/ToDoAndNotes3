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
using static ToDoAndNotes3.Controllers.ManageController;
using ToDoAndNotes3.Models.ManageViewModels;

namespace ToDoAndNotes3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly SignInManager<User> _signInManager;

        public HomeController(ILogger<HomeController> logger, TdnDbContext context, UserManager<User> userManager, 
            IAuthorizationService authorizationService, SignInManager<User> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _signInManager = signInManager;
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
            string? search = null, int? labelId = null, bool isGetPartial = false)
        {
            /*
               TempData["CurrentProjectId"]  => project select value on the Tasks/Notes.CreatePartial
               TempData["DaysViewName"] = daysViewName; => for default date on the Tasks/Notes.CreatePartial
               ViewData["ReturnUrl"] => important for the future partial-update-url generation
               ViewData["DisplayDataTitle"] => just for info about current view
             */

            string? userId = _userManager.GetUserId(User);
            var defaultProject = GetOrCreateDefaultProject();

            if (daysViewName is not null)
            {
                projectId = null;
                labelId = null;
                ViewData["ReturnUrl"] = Url.Action(nameof(Main), "Home", new { daysViewName });
                TempData["DaysViewName"] = daysViewName;
                ViewData["DisplayDataTitle"] = daysViewName;
            }
            if (projectId is not null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                  User, _context.Projects.Find(projectId), EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                daysViewName = null;
                labelId = null;
                ViewData["ReturnUrl"] = Url.Action(nameof(Main), "Home", new { projectId });
                ViewData["DisplayDataTitle"] = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId).Title;
                TempData["CurrentProjectId"] = projectId;
            }
            if (labelId is not null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                  User, _context.Labels.Find(labelId), EntityOperations.FullAccess);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                projectId = null;
                daysViewName = null;
                search = null;
                ViewData["ReturnUrl"] = Url.Action(nameof(Main), "Home", new { labelId });
                ViewData["DisplayDataTitle"] = "Label: " + _context.Labels.Find(labelId).Title;
            }           
            if (search is not null)
            {
                ViewData["Search"] = search;
                //ViewData["ReturnUrl"] += $"&search={search}";
                ViewData["ReturnUrl"] = Url.Action(nameof(Main), "Home", new { search });
                ViewData["DisplayDataTitle"] = "Search: " + search;
            }

            if ((TempData.Peek("CurrentProjectId") as int?) is null)
            {
                TempData["CurrentProjectId"] = defaultProject.ProjectId;
            }

            // sort things
            string? dateOrder = TempData.Peek("DateOrder") as string;
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

            // check data loading time
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            GeneralViewModel generalViewModel = await LoadGeneralViewModel(userId, daysViewName, projectId, labelId, search);

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            Console.WriteLine("Elapsed time: " + elapsedTime);

            SortGeneralViewModel(generalViewModel, dateOrder, hideCompleted);

            if (isGetPartial)
            {
                return PartialView("Home/_MainPartial", generalViewModel);
            }

            return View(generalViewModel);
        }

        // GET: /Home/SidebarPartial
        [HttpGet]
        public IActionResult SidebarPartial()
        {
            return PartialView("_SidebarPartial",
                _context.Projects.Where(p => p.UserId == _userManager.GetUserId(User) && p.IsDefault == false));
        }

        // GET: /Home/Labels
        [HttpGet]
        public async Task<IActionResult> Labels(bool isGetPartial = false)
        {
            ViewData["ReturnUrl"] = Url.Action(nameof(Labels), "Home");

            ProjectLabelViewModel projectLabelViewModel = new ProjectLabelViewModel();
            string? userId = _userManager.GetUserId(User);
            var projects = await _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToListAsync();
            var labels = await _context.Labels.Where(p => p.UserId == userId).ToListAsync();
            projectLabelViewModel.Projects = projects;
            projectLabelViewModel.Labels = labels;

            if (isGetPartial)
            {
                return PartialView("Home/_LabelsPartial", projectLabelViewModel.Labels);
            }
            else
            {
                return View(projectLabelViewModel);
            }
        }
        
        // GET: /Home/Bin
        [HttpGet]
        public async Task<IActionResult> Bin(bool isGetPartial = false)
        {
            ViewData["ReturnUrl"] = Url.Action(nameof(Bin), "Home");

            string userId = _userManager.GetUserId(User);

            // sort things
            string? dateOrder = TempData.Peek("DateOrder") as string;
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

            BinViewModel binViewModel = new BinViewModel();

            // all deleted items
            var projectsInclude = await _context.Projects
                .Where(p => p.UserId == userId)
                .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                .Include(n => n.Notes).ThenInclude(n => n.NoteLabels).ThenInclude(l => l.Label).IgnoreQueryFilters()
                .ToListAsync();

            // ActiveProjects (for sidebar)
            binViewModel.ActiveProjects = _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToList();
            // display all deleted projects
            binViewModel.DeletedProjects = _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false && p.IsDeleted == true).IgnoreQueryFilters().ToList();
            // display deleted tasks & notes (only if its project is active)
            foreach (var project in projectsInclude.Where(p => p.IsDeleted == false))
            {
                binViewModel.TdnElements.AddRange(project.Tasks.Where(t => t.IsDeleted == true));
                binViewModel.TdnElements.AddRange(project.Notes.Where(n => n.IsDeleted == true));
            }
            SortBinViewModel(binViewModel, dateOrder, hideCompleted);

            if (isGetPartial)
            {
                return PartialView("Home/_BinPartial", binViewModel);
            }
            else
            {
                return View(binViewModel);
            }
        }

        // GET: /Home/Bin
        [HttpGet]
        public async Task<IActionResult> Manage(ManageMessageId? message = null, bool isGetPartial = false)
        {
            ViewData["ReturnUrl"] = Url.Action(nameof(Manage), "Home");

            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            GeneralViewModel generalViewModel = await LoadGeneralViewModel(_userManager.GetUserId(User), daysViewName : DaysViewName.Today);

            var user = await _userManager.GetUserAsync(User);
            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                AuthenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user)
            };
            generalViewModel.Manage = model;

            if (isGetPartial)
            {
                return PartialView("Home/_ManagePartial", generalViewModel);
            }
            return View(generalViewModel);
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

        // POST: /Home/ChangeTempDataValueNoReload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ChangeTempDataValueNoReload(string? tempDataName, string? tempDataValue = null, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(tempDataName))
            {
                return;
            }
            TempData[tempDataName] = tempDataValue;
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
        private async Task<GeneralViewModel> LoadGeneralViewModel(string? userId, DaysViewName? daysViewName = null, int? projectId = null, int? labelId = null, string? search = null)
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
                    generalViewModel.TdnElements.AddRange(project.Tasks.Where(t => t.TaskLabels.Any(tl => tl.Label.LabelId == labelId)));
                    generalViewModel.TdnElements.AddRange(project.Notes.Where(n => n.NoteLabels.Any(nl => nl.Label.LabelId == labelId)));
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
                    generalViewModel.TdnElements.AddRange(project.Tasks.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase)));
                    generalViewModel.TdnElements.AddRange(project.Notes.Where(n => n.Title.Contains(search, StringComparison.OrdinalIgnoreCase)));
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
                    generalViewModel.TdnElements.AddRange(project.Tasks);
                    generalViewModel.TdnElements.AddRange(project.Notes);
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
                    generalViewModel.TdnElements.AddRange(project.Tasks);
                    generalViewModel.TdnElements.AddRange(project.Notes);
                }
            }
            else if (daysViewName == DaysViewName.Unsorted)
            {
                var defaultProject = GetOrCreateDefaultProject();

                // 1/2 load projects list separately to save resources
                generalViewModel.Projects = await _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToListAsync();
                // 2/2 load current project content separately to save resources
                var currentProjectInclude = _context.Projects
                    .Where(p => p.UserId == userId && p.ProjectId == defaultProject.ProjectId)
                    .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes).ThenInclude(n => n.NoteLabels).ThenInclude(l => l.Label)
                    .FirstOrDefault();

                generalViewModel.TdnElements.AddRange(currentProjectInclude.Tasks);
                generalViewModel.TdnElements.AddRange(currentProjectInclude.Notes);
            }
            else if (projectId is not null)
            {
                // 1/2 load projects list separately to save resources
                generalViewModel.Projects = await _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToListAsync();
                // 2/2 load current project content separately to save resources
                var currentProjectInclude = _context.Projects
                    .Where(p => p.UserId == userId && p.ProjectId == projectId)
                    .Include(t => t.Tasks).ThenInclude(t => t.TaskLabels).ThenInclude(l => l.Label)
                    .Include(n => n.Notes).ThenInclude(n => n.NoteLabels).ThenInclude(l => l.Label)
                    .FirstOrDefault();

                generalViewModel.TdnElements.AddRange(currentProjectInclude.Tasks);
                generalViewModel.TdnElements.AddRange(currentProjectInclude.Notes);
            }

            generalViewModel.Labels = await _context.Labels.Where(l => l.UserId == userId).ToListAsync();
            return generalViewModel;
        }
        private void SortGeneralViewModel(GeneralViewModel generalViewModel, string? dateOrder, bool? hideCompleted)
        {
            // sort by dateOrder, hideCompleted
            if (dateOrder == "ascending")
            {
                generalViewModel.TdnElements = generalViewModel.TdnElements.OrderBy(t => t.DueDate).ThenBy(t => t.DueTime).ToList();
                generalViewModel.TdnElements = generalViewModel.TdnElements.OrderBy(t => t.DueDate).ThenBy(t => t.DueTime).ToList();
            }
            else
            {
                generalViewModel.TdnElements = generalViewModel.TdnElements.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.DueTime).ToList();
                generalViewModel.TdnElements = generalViewModel.TdnElements.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.DueTime).ToList();
            }
            if (hideCompleted == true)
            {
                generalViewModel.TdnElements = generalViewModel.TdnElements.Where(t => t.IsCompleted == false).ToList();
            }
            else
            {
                generalViewModel.TdnElements = generalViewModel.TdnElements.OrderBy(t => t.IsCompleted).ToList();
            }
        }
        private void SortBinViewModel(BinViewModel binViewModel, string? dateOrder, bool? hideCompleted)
        {
            // sort by dateOrder, hideCompleted
            if (dateOrder == "ascending")
            {
                binViewModel.TdnElements = binViewModel.TdnElements.OrderBy(t => t.DueDate).ThenBy(t => t.DueTime).ToList();
                binViewModel.TdnElements = binViewModel.TdnElements.OrderBy(t => t.DueDate).ThenBy(t => t.DueTime).ToList();
            }
            else
            {
                binViewModel.TdnElements = binViewModel.TdnElements.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.DueTime).ToList();
                binViewModel.TdnElements = binViewModel.TdnElements.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.DueTime).ToList();
            }
            if (hideCompleted == true)
            {
                binViewModel.TdnElements = binViewModel.TdnElements.Where(t => t.IsCompleted == false).ToList();
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
