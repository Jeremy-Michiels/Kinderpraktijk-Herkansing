using Microsoft.AspNetCore.Mvc;

namespace src.Controllers
{
    public class Wwz : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
