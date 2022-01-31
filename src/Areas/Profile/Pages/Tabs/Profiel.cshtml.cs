using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using src.Areas.Profile.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using AutoMapper;
using System.Collections.Generic;
using src.Models;
using System.Threading.Tasks;
using System;

namespace src.Areas.Profile.Pages.Tabs
{
    public class ProfielModel : PageModel
    {
        private readonly MijnContext _context;
        private readonly UserManager<srcUser> _userManager;
        private readonly IMapper _mapper;

        public ProfielModel(MijnContext context, UserManager<srcUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public List<ProfileViewModel> ProfileViewModel { get; set; } = new List<ProfileViewModel>();
        //Dit is het profiel van de ingelogde gebruiker
        public ProfileViewModel MijnProfiel { get; set; }
        //Dit geeft een lijst weer van alle aanmeldingen     
        public List<Aanmelding> Aanmeldingen {get;set;} = new List<Aanmelding>();    
        //Dit geeft set de CurrentUser
        public srcUser CurrentUser{get;set;}

        //Deze worden automatisch geset
        //Door deze filter zie je al gelijk de dingen die geactiveerd zijn
        [BindProperty]
        public bool Aangemeld { get; set; }
        [BindProperty]
        public bool Afgemeld { get; set; }

        public string SpecialistName { get; set; }
        

        public async Task OnGetAsync(bool aan, bool af)
        {
            //Als er een filter functie is deze aanpassen
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _context.Users.Where(x => x.Id == currentUserId).FirstOrDefaultAsync();
            SetCurrentUser(currentUserId);
            //Hiermee wordt een lijst met de actieve aanmeldingen gegenereerd
            //Daarvoor moet je de speciale FilterList aanroepe
            if(User.IsInRole("Pedagoog") || User.IsInRole("Assistent")){
                Aanmeldingen = GetAanmeldingen(currentUserId).ToList();
            }else{
                Aanmeldingen = GetFilters(GetAanmeldingen(),aan,af).ToList();
            }
        }

        public async Task<IActionResult> OnPostMeldAan(string id)
        {
            CurrentUser = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
            CurrentUser.SpecialistId = _userManager.GetUserId(User);
            //hier wordt de laatste aanmelding die is gemaakt geopend
            var LaasteAanmelding = GetAanmeldingen(CurrentUser.SpecialistId)
                                                    .OrderByDescending(x=>x.Id)
                                                     .First();
            
            //Hier wordt de laatste aanmelding op waar gezet
            LaasteAanmelding.IsAangemeld = true;
            LaasteAanmelding.AfmeldingDatum =  DateTime.Now;
            
            //Hieronder wordt een nieuwe chat gemaakt tussen de pedagoog en de gebruiker
            var Pedagoog = _context.Users.Where(x=>x.Id==CurrentUser.SpecialistId).SingleOrDefault();
            await CreateNewGroupAsync(CurrentUser,Pedagoog);

            //Hier wordt de aanmelding die gedaan wordt opgeslagen
            _context.Aanmeldingen.Update(LaasteAanmelding);
            _context.Users.Update(CurrentUser);
            _context.SaveChanges();
            return RedirectToPage("/Tabs/Profiel", new { Area = "Profile" });
        }

        public async Task<IActionResult> OnPostMeldAf(string id)
        {
            CurrentUser = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();

            var LaasteAanmelding = GetAanmeldingen(CurrentUser.SpecialistId)
                                                    .OrderByDescending(x=>x.Id)
                                                     .First();

            CurrentUser.SpecialistId = null;

            //Hier chat verwijderen
            DeletePrivateGroup(CurrentUser);

            //hier wordt de afmelding waar gemaakt
            LaasteAanmelding.IsAfgemeld = true;
            LaasteAanmelding.AfmeldingDatum = DateTime.Now;

            _context.Aanmeldingen.Update(LaasteAanmelding);
            _context.Users.Update(CurrentUser);
            _context.SaveChanges();
            return RedirectToPage("/Tabs/Profiel", new { Area = "Profile" });
        }

        public IActionResult OnPostFilter(bool aan, bool af)
        {
            Aangemeld = aan;
            Afgemeld = af;

            return RedirectToPage("/Tabs/Profiel", new { Area = "Profile", aan = Aangemeld, af = Afgemeld});
        }
        public void SetCurrentUser(string currentUserId){
            CurrentUser = _context.Users
                                        .Include(x=>x.Childeren)
                                        .Where(x=>x.Id== currentUserId).SingleOrDefault();
        }
        //Hier wordt een lijst met kinderen opgevraagd
        public async Task<List<srcUser>> GetChildAsync(string currentUserId){ 
           return await _context.Users
                                        .Where(x => x.ParentId == currentUserId)
                                        .ToListAsync();
        }
        //Hier kan je filters toevoegen van aan de Aanmeldingen lijst
        public IQueryable<Aanmelding> GetFilters(IQueryable<Aanmelding> lijst,bool aan, bool af){
                return lijst.Where(x=>x.IsAangemeld==aan).Where(x=>x.IsAfgemeld==af);
        }
        //Hier wordt een lijst van alle aanmeldingen gegeven
        public IQueryable<Aanmelding> GetAanmeldingen(string PedagoogId){
                return _context.Aanmeldingen
                .Include(x=>x.Client)
                                            .Include(x=>x.Pedagoog)
                                            .Where(x=>x.PedagoogId==PedagoogId);
                                            
        }
        public IQueryable<Aanmelding> GetAanmeldingen(){
                return _context.Aanmeldingen
                                            .Include(x=>x.Client)
                                            .Include(x=>x.Pedagoog);
        }
        //Deze is voor het aanmaken van een prive chat
        [HttpPost]
        public async Task<bool> CreateNewGroupAsync(srcUser user, srcUser pedagoog){
            Chat chat = new Chat(){Naam="Prive chat "+user.LastName,Beschrijving="Dit is de prive chat tussen jou en de pedagoog", type=ChatType.Private};
            chat.Users = new List<ChatUser>(){
                new ChatUser(){UserId= user.Id, Role=UserRole.Member,ChatId = chat.Id},
                new ChatUser(){UserId=pedagoog.Id, Role=UserRole.Admin, ChatId = chat.Id}
            };
            _context.Chat.Add(chat);
            await _context.SaveChangesAsync();
            return true;
        }
        //Deze is voor het verwijderen van een prive chat tussen 2 groepen
        [HttpPost]
        public bool DeletePrivateGroup(srcUser client){
            var chat = getPriveChat(client.Id);
                //Dit is om alle verbindingen die gemaakt zijn met de chat ook gelijk worden verwijderd
                foreach(var item in _context.ChatUsers.Where(x=>x.ChatId==chat)){
                        _context.ChatUsers.Remove(item);
                }
                _context.Chat.Remove(_context.Chat.Where(x=>x.Id==chat).Single());
                _context.SaveChanges();
                return true;
            }
        public int getPriveChat(string userId){
            var ChatUser = _context.ChatUsers.Include(x => x.chat)
                .Where(x => x.UserId == userId)
                .Where(x => x.chat.type == ChatType.Private)
                .SingleOrDefault();
            return ChatUser.ChatId;
        }
    }
}
