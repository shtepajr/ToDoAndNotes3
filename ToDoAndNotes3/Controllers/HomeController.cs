using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Main()
        {
            //SeedDbData();
            GeneralViewModel generalViewModel = new GeneralViewModel();
            generalViewModel.Projects = _context.Projects.ToList();
            generalViewModel.Tasks = _context.Tasks.ToList();
            generalViewModel.Notes = _context.Notes.ToList();
            generalViewModel.Labels = _context.Labels.ToList();

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
            for (int i = 0; i < 10; i++)
            {
                _context.Projects.Add(
                new Project()
                {
                    UserId = _userManager.GetUserId(User),
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    Title = "Lorem ipsum hello",
                    Tasks = new List<Models.Task>()
                    {
                        new Models.Task()
                        {
                            Title = "Task title test",
                            Description = "Desc"
                        }
                    }
                });
            }            
            _context.SaveChanges();
        }
        #endregion
    }
}
