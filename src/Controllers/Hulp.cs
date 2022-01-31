using Microsoft.AspNetCore.Mvc;

namespace src.Controllers
{
    public class Hulp : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
