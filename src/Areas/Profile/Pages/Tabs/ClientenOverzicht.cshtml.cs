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
    [Authorize(Roles = "Pedagoog, Assistent")]

    public class ClientenOverzichtModel : PageModel
    {
        private readonly MijnContext _context;
        private readonly UserManager<srcUser> _usermanager;

        public ClientenOverzichtModel(MijnContext context, UserManager<srcUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        public List<srcUser> users;

        // ophalen van lijst met clienten
        public void OnGet()
        {
            string specialistId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(User.IsInRole("Assistent")){
                var currentUserId = _usermanager.GetUserId(User);
                var currentUser = _context.Users.Where(x => x.Id == currentUserId).SingleOrDefault();
                var NaarSpecialist = currentUser.SpecialistId;
                
                specialistId = _context.Users.Where(x => x.Id == NaarSpecialist).Select(x => x.Id).SingleOrDefault();
            }
            var list2 = _context.UserRoles.Where(p => p.RoleId == "5" ).Select(x => x.UserId);
            users = _context.Users.Where(p => p.SpecialistId == specialistId).ToList();
            users.RemoveAll(x => list2.Contains(x.Id));
        }
    }
}