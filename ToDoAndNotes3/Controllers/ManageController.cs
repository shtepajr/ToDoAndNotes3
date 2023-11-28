using Microsoft.AspNetCore.Mvc;

namespace ToDoAndNotes3.Controllers
{
    public class ManageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
