using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToDoAndNotes3.Data;
using ToDoAndNotes3.Models;

namespace ToDoAndNotes3.Controllers
{
    public class LabelsController : Controller
    {
        private readonly TdnDbContext _context;
        private readonly UserManager<User> _userManager;

        public LabelsController(TdnDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var labels = _context.Labels.Where(l => l.UserId == _userManager.GetUserId(User));
            return PartialView("Labels/_IndexPartial", labels);
        }
    }
}
