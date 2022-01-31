using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using src.Areas.Profile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace src.Areas.Profile.Pages.Tabs
{

    public class ViewSpecialistModel : PageModel
    {

        private readonly MijnContext _context;
        private readonly UserManager<srcUser> _userManager;
        private readonly IMapper _mapper;

        public ViewSpecialistModel(MijnContext context, UserManager<srcUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        [BindProperty]
        public List<ProfileViewModel> ProfileViewModel { get; set; }
        public bool checkAanmelding { get; set; }

        public void OnGet()
        {
            var currentUserID = _userManager.GetUserId(User);
            var result = (from s in _context.Users
                          where !String.IsNullOrEmpty(s.Specialism)
                          select s).ToList();

            ProfileViewModel = _mapper.Map<List<srcUser>, List<ProfileViewModel>>(result);


            checkAanmelding = _context.Aanmeldingen
                                                .Where(x => !x.IsAfgemeld)
                                                .Any(x => x.ClientId == currentUserID);
            /*                            
    checkAanmelding = (from l in _context.Aanmeldingen
                           where !String.IsNullOrEmpty(_userManager.GetUserId(User))
                           where !String.IsNullOrEmpty(_userManager.GetUserId(User)) && (l.IsAangemeld.Equals(false) && l.IsAfgemeld.Equals(false))
                           select l).Any(); 
                           */
        }

        public async Task<IActionResult> OnPost(string id)
        {
            var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToPage("/Tabs/ViewSpecialist", new { Area = "Profile" });
        }

        public async Task<IActionResult> OnPostRegister(string id)
        {
            var date = DateTime.Now;
            var currentUser = _userManager.GetUserId(User);
            Aanmelding aanmelding = new Aanmelding { AanmeldingDatum = date, ClientId = currentUser, PedagoogId = id };
             _context.SaveChanges();
            _context.Aanmeldingen.Add(aanmelding);
            _context.SaveChanges();
            return RedirectToPage("/Tabs/ViewSpecialist", new { Area = "Profile" });
        }

    }
}
