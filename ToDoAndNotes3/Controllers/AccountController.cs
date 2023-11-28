using Microsoft.AspNetCore.Mvc;

namespace ToDoAndNotes3.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
