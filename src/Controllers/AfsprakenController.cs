using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace src.Controllers
{
    [Authorize(Roles ="Assistent, Pedagoog")]
    public class AfsprakenController : Controller
    {
        private readonly MijnContext _context;
        private readonly UserManager<srcUser> _userManager;

        public AfsprakenController(MijnContext context, UserManager<srcUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Afspraken
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = _context.Users.Where(x => x.Id == currentUserId).SingleOrDefault();
            var mijnContext = _context.Users.Where(x => x.SpecialistId == currentUser.Id);

            return View(await mijnContext.ToListAsync());
        }
    }
}
