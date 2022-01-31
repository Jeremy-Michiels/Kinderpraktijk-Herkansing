using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Pedagoog , Moderator, Assistent")]
public class ClientController: Controller{
private MijnContext _context;
  private srcUser _currentUser;
 public srcUser currentUser 
 {
    get { 
        if(_currentUser==null){
            var CurrentUserId=User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _currentUser =_context.Users.Where(x=>x.Id==CurrentUserId).Single();
        }
        return _currentUser;
        }
    set { _currentUser = value;}
 }
    
    public ClientController(MijnContext context){
        _context =context;
    }

    public ActionResult Index(string zoek){
        
        ViewData["ZoekTerm"] = zoek;
        //hiermee worden alle privé chats toegevoegd
        return View(ZoekOp(GetClients(),zoek).ToList());
        
    }
    public IQueryable<Chat> ZoekOp(IQueryable<Chat> lijst, string trefwoord){
        if(trefwoord==null||trefwoord==""){
            return lijst;
        }
        return lijst.Where(x=>x.Naam.Contains(trefwoord));
    }
    public IQueryable<Chat> GetClients(){
         //Hier in de list wordt gekeken of de users in de chat zitten.
        //dan wordt gekeken of de chat een private chat is
        var CurrentUser =User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(User.IsInRole("Assistent")){
                var currentUserId = _currentUser.Id;
                var currentUser = _context.Users.Where(x => x.Id == currentUserId).SingleOrDefault();
                var NaarSpecialist = currentUser.SpecialistId;
                
                CurrentUser = _context.Users.Where(x => x.Id == NaarSpecialist).Select(x => x.Id).SingleOrDefault();
            }
        return _context.ChatUsers.Include(x=>x.chat)
                                                                        .Where(x=>x.UserId==CurrentUser)
                                                                        .Select(x=>x.chat)
                                                                        .Where(x=>x.type==ChatType.Private);
    }


  //TODO tests maken voor deze room
    [HttpPost]
    [Authorize(Roles = "Moderator,Pedagoog")]
    public async Task<IActionResult> CreateRoom([Bind("Naam","Beschrijving")]Chat chat){
        if(ModelState.IsValid){
            chat.type = ChatType.Private;
            chat.Users = new List<ChatUser>();
            chat.Users.Add(new ChatUser(){
                UserId =User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Admin,
                ChatId = chat.Id
            });  
            //Hieronder moet een een user aangemaakt worden voor degene waarvoor de chat is
            _context.Chat.Add(chat);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }else{
            Console.WriteLine("modelstate is niet valid");
        }
        return View();
    }
}