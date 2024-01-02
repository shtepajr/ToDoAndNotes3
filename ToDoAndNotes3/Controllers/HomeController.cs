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

        public HomeController(ILogger<HomeController> logger, TdnDbContext context, UserManager<User> userManager = null)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Main(int? currentProjectId = null)
        {
            GeneralViewModel generalViewModel = new GeneralViewModel();
            string? userId = _userManager.GetUserId(User);

            //SeedDbData();
            if (currentProjectId == null)
            {
                var projectsInclude = await _context.Projects.Where(p => p.UserId == userId)
                    .Include(t => t.Tasks).Include(n => n.Notes).ToListAsync();
                generalViewModel.Projects = projectsInclude;

                foreach (var project in projectsInclude)
                {
                    generalViewModel.Tasks.AddRange(project.Tasks);
                    generalViewModel.Notes.AddRange(project.Notes);
                }
            }
            else
            {
                var projects = await _context.Projects.Where(p => p.UserId == userId).ToListAsync();
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
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Helpers
        void SeedDbData()
        {
            if (_context.Projects.Any())
            {
                _context.Projects.RemoveRange(_context.Projects);
            }
            //_context.Database.EnsureDeleted();
            //_context.Database.EnsureCreated();

            for (int i = 0; i < 5; i++)
            {
                _context.Labels.Add(new Label()
                {
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
