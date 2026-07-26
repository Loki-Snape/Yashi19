using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class ContactUSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
