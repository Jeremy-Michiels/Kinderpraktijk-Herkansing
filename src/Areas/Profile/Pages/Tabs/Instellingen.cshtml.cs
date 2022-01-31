using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace src.Areas.Profile.Pages.Tabs
{
    public class InstellingenModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/Account/Manage/Index", new { Area = "Identity" });
            //return RedirectToPage("/Tabs/Specialist", new { Area = "Profile" });
        }
    }
}
