using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class DashboardController : Controller{
    private MijnContext _context;
    private static bool success = false;
    private static string message;
    public DashboardController(MijnContext context){
        //Deze moet achteraf ook aangepast worden door de database die we van plan zijn
        //te gebruiken
        _context = context;
    }
    //Getest
    public IActionResult Index(string onderwerp, string leeftijdscategorie,bool? m){
        if(onderwerp != null)
        ViewData["Onderwerp"] = onderwerp;
        if(leeftijdscategorie != null)
        ViewData["Leeftijdscategorie"] = leeftijdscategorie;
        if(User.IsInRole("Ouder"))
        {
            return RedirectToAction("Overzicht");
        }
        //Dit heeft betrekking op het plaatsten van een annonieme melding
        //Als er een annonieme melding is geplaatst dan geeft deze de melding weer
        ViewData["meldingGeplaatst"] = m==null?false: true;

        string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        srcUser user = _context.Users.Where(p => p.Id == UserId).FirstOrDefault();
        if(user.UserBlocked)
        {
            return RedirectToAction("NotAuthorized");
        }
        
        //Hier wordt meegegeven of de user een moderator is.
        //op basis hiervan wordt bepaald of de user te zien krijgt of hij een groep aan mag maken of dat hij kan chatten met de pedagoog
        ViewData["IsModerator"] = User.IsInRole("Moderator")||User.IsInRole("Pedagoog");
        ViewData["IsAssistent"] = User.IsInRole("Assistent");
        //Onderstaande viewdata is voor het weergeven van de button
        ViewData["HeeftPriveChat"] = heeftPriveChat();
        
        ViewData["Success"] = success;
        ViewData["SuccessMessage"] = message;
        success = false;
        message = "";

        //met deze method haal het Id van de current User op
        var CurrentUser =User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //Hier wordt het zoekfilter van onderwerp en leeftijdscategorie toegevoegd        
        return View(LeeftijdsCatagorie(Onderwerp(GetChats(),onderwerp),leeftijdscategorie).ToList());
    }
    //Dit is voor het filteren van de titel uit de lijst
    public IQueryable<Chat> FilterTitel(IQueryable<Chat> lijst,string titel){
        if(titel == null || titel == ""){
            return lijst;
        }
        return lijst.Where(x => x.Naam.Contains(titel));
    }
    public IQueryable<Chat> FilterBeschrijving(IQueryable<Chat> lijst,string beschrijving){
        if(beschrijving == null || beschrijving == ""){
            return lijst;
        }
        return lijst.Where(x => x.Beschrijving.Contains(beschrijving));
    }

    //Dit is de sorteer Query van de leeftijd waarop gesorteerd kan worden
    public IQueryable<Chat>  LeeftijdsCatagorie(IQueryable<Chat> lijst,string leeftijdsCatagorie){
        if(leeftijdsCatagorie == null || leeftijdsCatagorie == ""){
            return lijst;
        }
        return lijst.Where(x => x.Leeftijdscategorie == leeftijdsCatagorie);
    }
    //Dit is de Query van het onderwerp waarop gesorteerd kan worden
    public IQueryable<Chat>  Onderwerp(IQueryable<Chat> lijst,string onderwerp){
        if(onderwerp == null || onderwerp == ""){
            return lijst;
        }
        return lijst.Where(x => x.Onderwerp == onderwerp);
    }
    //Met onderstaande methode wordt gekeken in welke lists de user staat ingeschreven
    public IQueryable<Chat> GetChats(){
        var CurrentUser =User.FindFirst(ClaimTypes.NameIdentifier).Value;
        return _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==CurrentUser).Select(x=>x.chat).Where(x=>x.type==ChatType.Room);
    }
    public IQueryable<Chat> GetAllChats(){
        return _context.Chat.Where(x=>x.type==ChatType.Room);
    }

    //Dit is voor het verkrijgen van de view voor het toevoegen van de model
    [HttpGet]
    [Authorize(Roles = "Moderator,Pedagoog,Client")]
    public IActionResult GroepToevoegen(string Naam, string Beschrijving, string onderwerp, string Leeftijdscategorie){

        //Hier wordt die lijst terug geven aan de mensen
        return View(LeeftijdsCatagorie(Onderwerp(FilterBeschrijving(FilterTitel(GetAllChats(),Naam),Beschrijving),onderwerp),Leeftijdscategorie).ToList());
    }

    //Deze is voor de chat zelf. Hiermee kan je alle berichten zien
    [HttpGet]
    [Authorize(Roles = "Moderator,Pedagoog,Client")]
    public IActionResult Chat(int ChatId){
        if(UserIsIn(ChatId)){
            ViewData["IsModerator"] = User.IsInRole("Moderator")||User.IsInRole("Pedagoog");
            return View(_context.Chat.Include(x=>x.Messages).Where(x=>x.Id==ChatId).Single());
        }
        return RedirectToAction("Index",_context.Chat);
    }

    [Authorize(Roles = "Moderator,Pedagoog")]
    public IActionResult MaakZelfhulpgroep(){
            return View();
    }
    //Deze methode is voor het aanmaken van een Chatroom
    [HttpPost]
    [Authorize(Roles = "Moderator,Pedagoog")]
    public async Task<IActionResult> CreateRoom([Bind("Naam","Beschrijving","Onderwerp","Leeftijdscategorie")]Chat chat){
        if(ModelState.IsValid){
            chat.type = ChatType.Room;
            chat.Users = new List<ChatUser>();
            chat.Users.Add(new ChatUser(){
            UserId =User.FindFirst(ClaimTypes.NameIdentifier).Value,
            Role = UserRole.Admin,
            ChatId = chat.Id
            });
            _context.Chat.Add(chat);
            await _context.SaveChangesAsync();
            success = true;
            message = "De groep is successvol aangemaakt";
            return RedirectToAction("Index");
        }else{
            Console.WriteLine("modelstate is niet valid");
        }
        return View();
    }
    //Deze methode laat de gegevens van de huidige chat zien
    //Dit is een get Als een user hier geen toegang tot heeft wordt hij ook geweigerd
    [HttpGet]
    [Authorize(Roles = "Moderator,Pedagoog,Admin,Client")]
    public IActionResult Details(int chatId){
        //Misschien voor de zekerheid een check hier doen
        if(UserIsIn(chatId)){
            ViewData["Users"] =  _context.Chat.Where(x=>x.Id==chatId).Include(x=>x.Users).Single().Users.Count();
            ViewData["IsPedagoog"] = User.IsInRole("Moderator")||User.IsInRole("Pedagoog");
            return View(_context.Chat.Where(x=>x.Id==chatId).Single());
        }
        return RedirectToAction("NotAuthorized");
    }

    //Deze methode is verantwoordelijk voor het verwijderen van een chat uit de chat list
    //Mischien een extra vertificatie toevoegen
    [HttpGet]
    [Authorize(Roles = "Moderator,Pedagoog")]
    public IActionResult DeleteRoom(int Id,bool error){
        if(UserIsIn(Id)){
            ViewData["Gelukt"] = !error;
            return View(_context.Chat.Where(x=>x.Id==Id).Single());
            }
        return RedirectToAction("NotAuthorized");
    }
    [HttpPost]
    [Authorize(Roles = "Moderator,Pedagoog")]
    public IActionResult DeleteRoom(int ChatRoomId,string ChatRoomName){
        if(UserIsIn(ChatRoomId)){
            var chat = _context.Chat.Find(ChatRoomId);
            if(chat.Naam.Equals(ChatRoomName)){
                //Dit is om alle verbindingen die gemaakt zijn met de chat ook gelijk worden verwijderd
                foreach(var item in _context.ChatUsers.Where(x=>x.ChatId==ChatRoomId)){
                        _context.ChatUsers.Remove(item);
                }
                _context.Chat.Remove(chat);
                _context.SaveChanges();
                success = true;
                message = "De groep is successvol verwijderd";
                return RedirectToAction("Index");
            }else{
                return RedirectToAction("DeleteRoom",new{Id= chat.Id , error=true});
            }
        }
        return RedirectToAction("NotAuthorized");
    }
    [HttpGet]
    [Authorize(Roles = "Moderator,Pedagoog,Client")]
    public IActionResult RemoveRoomFromList(int Id,bool error){
        if(UserIsIn(Id)){
            ViewData["Gelukt"] = error;
            return View(_context.Chat.Where(x=>x.Id==Id).Single());
            }
        return RedirectToAction("NotAuthorized");
    }
    //Remove room from list
    //Als je een owner bent van de 
    [HttpPost]
    [Authorize(Roles = "Moderator,Pedagoog,Client")]
    public IActionResult RemoveRoomFromList(string ChatRoomName, int ChatRoomId){
        if(UserIsIn(ChatRoomId)){
            if(_context.Chat.Where(x=>x.Id==ChatRoomId).Single().Naam!=ChatRoomName){
                return RedirectToAction("RemoveRoomFromList", new{Id=ChatRoomId,error=true});
            }
            var userid =User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var ChatUser = _context.ChatUsers.Where(x=>x.ChatId==ChatRoomId).Where(x=>x.UserId==userid).Single();
            _context.ChatUsers.Remove(ChatUser);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }
    //Edit room
    [HttpGet]
    [Authorize(Roles = "Moderator,Pedagoog")]
    public IActionResult Edit(int Id){
        if(UserIsIn(Id)){
            return View(_context.Chat.Where(x=>x.Id==Id).SingleOrDefault());
        }
        return RedirectToAction("NotAuthorized");
    }
    [HttpPost]
    [Authorize(Roles = "Moderator,Pedagoog")]
    public IActionResult Edit(int Id,string naam,string beschrijving){
        if(UserIsIn(Id)){
            var chat = _context.Chat.Where(x=>x.Id==Id).SingleOrDefault();
            chat.Naam = naam;
            chat.Beschrijving = beschrijving;
            _context.Chat.Update(chat);
            _context.SaveChanges();
            success = true;
            message= "De groep is successvol bewerkt";
            return RedirectToAction("Details",new{chatId=Id});
        }
        return RedirectToAction("NotAuthorized");
    }
    
    [HttpPost]
    [Authorize(Roles = "Moderator,Pedagoog,Client")]
    public async Task<IActionResult> JoinChat(int id){
        if(!UserIsIn(id)){
            var ChatUser = new ChatUser(){
            ChatId = id,
            UserId =User.FindFirst(ClaimTypes.NameIdentifier).Value,
            Role = UserRole.Member,
            };
            _context.ChatUsers.Add(ChatUser);
            await _context.SaveChangesAsync();
            Console.WriteLine("Er is een nieuwe user aan de groep "+ id+ " toegevoegd");
        }
        return RedirectToAction("Chat",new {ChatId = id});
    }
    public IActionResult GaNaarPriveChat(){
        if(heeftPriveChat()){
            var userid =User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var ChatUser = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userid).Where(x=>x.chat.type==ChatType.Private).SingleOrDefault();
            return RedirectToAction("Chat",new {ChatId = ChatUser.ChatId}); //Deze doet het niet
        }
        return RedirectToAction("Index"); //Deze moet ook nog een foutmelding returnen
    }
    public bool heeftPriveChat(){
        var userid =User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if(!User.IsInRole("Pedagoog")){
            var ChatUser = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userid).Where(x=>x.chat.type==ChatType.Private).SingleOrDefault();
            return ChatUser!=null;
        }
        return false;
    }
    
    [Authorize(Roles="Ouder, Moderator")]
    public async Task<IActionResult> Overzicht()
    {
        string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var kid = _context.Users.Where(p => p.ParentId == currentUserId).FirstOrDefault(); //Deze is null
        IList<Chat> chatsChildCurrentUser = new List<Chat>();
        if(kid!=null){
             chatsChildCurrentUser = _context.Chat.Include(p => p.Messages)
                .Include(x => x.Users)
                .Where(p => p.Users.All(p => p.UserId == kid.Id))
                .ToList();
        }
        ViewData["ChatsLijst"] = chatsChildCurrentUser;
        return View(await _context.Users.Include(x=>x.Chats).Where(p => p.ParentId == currentUserId).ToListAsync());
    }
    public ActionResult NotAuthorized(){
        return View();
    }

    //Door onderstaande kan je zien of een user in een bepaalde class is
    public bool UserIsIn(int ChatId){
        //Hier wordt de current user opgevraagd
        var CurrentUser =User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //hieronder wordt gekeken of de user in de lijst zit van alle users van de aangegeven chat
        return _context.ChatUsers.Where(x=>x.ChatId==ChatId).Any(x=>x.UserId==CurrentUser);
    }
}