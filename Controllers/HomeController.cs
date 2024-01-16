using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            return View();
        }

        // GET: /Home/Main
        public async Task<IActionResult> Main(int? currentProjectId = null, DaysViewName? daysViewName = null)
        {
            GeneralViewModel generalViewModel = new GeneralViewModel();
            string? userId = _userManager.GetUserId(User);

            if (currentProjectId == null)
            {
                var defaultProject = GetOrCreateDefaultProject();
                TempData["CurrentProjectId"] = defaultProject.ProjectId;

                if (daysViewName == DaysViewName.Upcoming)
                {
                    var projectsUpcomingInclude = await _context.Projects.Where(p => p.UserId == userId)
                        .Include(t => t.Tasks).Include(n => n.Notes).ToListAsync();
                    generalViewModel.Projects = projectsUpcomingInclude;

                    foreach (var project in projectsUpcomingInclude)
                    {
                        generalViewModel.Tasks.AddRange(project.Tasks.Where(t => t.DueDate != null));
                        generalViewModel.Notes.AddRange(project.Notes.Where(t => t.DueDate != null));
                    }
                }
                else // today data by default
                {
                    var projectsTodayInclude = await _context.Projects.Where(p => p.UserId == userId)
                        .Include(t => t.Tasks).Include(n => n.Notes).ToListAsync();
                    generalViewModel.Projects = projectsTodayInclude;

                    foreach (var project in projectsTodayInclude)
                    {
                        var today = DateOnly.FromDateTime(DateTime.Now);
                        generalViewModel.Tasks.AddRange(project.Tasks.Where(t => t.DueDate == today));
                        generalViewModel.Notes.AddRange(project.Notes.Where(t => t.DueDate == today));
                    }
                }
            }
            else
            {
                TempData["CurrentProjectId"] = currentProjectId;
                // authorization
                var projects = await _context.Projects.Where(p => p.UserId == userId && p.IsDefault == false).ToListAsync();
                generalViewModel.Projects = projects;

                var currentProjectInclude = _context.Projects
                    .Where(p => p.UserId == userId && p.ProjectId == currentProjectId)
                    .Include(t => t.Tasks).Include(n => n.Notes)
                    .First();

                generalViewModel.Tasks.AddRange(currentProjectInclude.Tasks);
                generalViewModel.Notes.AddRange(currentProjectInclude.Notes);
            }
           
            return View(generalViewModel);
        }

        // GET: /Home/Labels
        public async Task<IActionResult> Labels()
        {
            ProjectLabelViewModel projectLabelViewModel = new ProjectLabelViewModel();
            string? userId = _userManager.GetUserId(User);
            var projects = await _context.Projects.Where(p => p.UserId == userId).ToListAsync();
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

        #region Helpers
        Project GetOrCreateDefaultProject()
        {
            var checkDefault = _context.Projects.Where(p => p.IsDefault == true).FirstOrDefault();
            if (checkDefault == null)
            {
                var defaultProject = new Project()
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
                _context.Projects.Add(new Project()
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
