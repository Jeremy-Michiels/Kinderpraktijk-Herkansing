using Microsoft.AspNetCore.Mvc;

namespace src.Controllers
{
    public class Contact : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
