using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using src.Areas.Profile.ViewModels;

namespace src.Areas.Profile.Pages.Tabs
{
    [Authorize(Roles = "Pedagoog")]

    public class ClientenOverzichtModel : PageModel
    {
        private readonly MijnContext _context;

        public ClientenOverzichtModel(MijnContext context)
        {
            _context = context;
        }

        public List<srcUser> users;

        // ophalen van lijst met clienten
        public void OnGet()
        {
            string specialistId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            users = _context.Users.Where(p => p.SpecialistId == specialistId).ToList();
        }
    }
}