using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = "Moderator")]
    public class GebruikersOverzichtModel : PageModel
    {
        private readonly MijnContext _context;

        public GebruikersOverzichtModel(MijnContext context)
        {
            _context = context;
        }

        public List<srcUser> users;

        public void OnGet()
        {
            users = _context.Users.ToList();
        }

        public void Blokkeren(srcUser user)
        {
            if(user.UserBlocked)
            {
                user.UserBlocked = false;
                _context.SaveChangesAsync();
            } else {
                user.UserBlocked = true;
                _context.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> OnPost(string id)
        {
            srcUser user = await _context.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
            Blokkeren(user);
            return RedirectToPage("/Tabs/GebruikersOverzicht");
        }
    }
}