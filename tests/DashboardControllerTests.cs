using System;
using Xunit;
using src.Models;
using Moq;
using src.Areas.Identity.Data;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace tests
{
    public class DashboardControllerTests
    {
        private MijnContext GetDatabase()
        {
            MockDatabase m = new MockDatabase();
            return m.CreateContext();
        }
        //ClaimTypeId is denk een soort van static userId
        private DashboardController getController(MijnContext context, string roleClaim, string ClaimTypeId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, roleClaim),
                new Claim(ClaimTypes.NameIdentifier,ClaimTypeId)
            }, "mock"));

            var controller = new DashboardController(context);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            return controller;
        }
        //Hieronder staan strings die worden gebruikt als roletoekenning.
        //Als er een role in naam veranderd kan het op deze manier makkelijker ontdekt worden
        private string Ouder = "Ouder";
        private string Client = "Client";
        private string Pedagoog = "Pedagoog";
        private string Moderator = "Moderator";

        //Index Tests\\
        //Met deze test, test ik of als je de role hebt van ouder dat deze dan geridirect wordt naar de overzichtspagina
        [Fact]
        public void IndexOuderTest()
        {
            //arrange
            DashboardController controller = getController(GetDatabase(),Ouder,"User1");
            var index = controller.Index("","",false);
            //act
            var IndexActionResult = Assert.IsType<RedirectToActionResult>(index);
            //assert
            Assert.Equal("Overzicht", IndexActionResult.ActionName);
        }
        [Theory]
        [InlineData("Moderator", "True")]
        [InlineData("Pedagoog", "True")]
        [InlineData("Client", "False")]
        public void IndexTestIsModerator(string role, string expected)
        {
            DashboardController controller = getController(GetDatabase(), role, "User1");
            IActionResult index = controller.Index("","",false);
            //Dit laad hij als een test die er niet staat
            //var IndexActionResult =Assert.IsType<IActionResult>(index);

            ViewResult viewResult = index as ViewResult;

            var viewData = viewResult.ViewData["IsModerator"];

            Assert.Equal(expected, viewData + "");
        }
        //BIj deze test wordt getest het aantal correcte chats opgehaald worden
        //Als het filteren en sorteren getest moet worden moeten daar ook performance testen bij gemaakt worden
        [Theory]
        [InlineData("User1",1)]
        [InlineData("User2",2)]
        [InlineData("User3",2)]
        [InlineData("User4",0)]
        public void TestIndexChatList(string user, int ChatAmount){ 
        DashboardController controller = getController(GetDatabase(),Pedagoog,user);

            var result = controller.Index("","",false);

            ViewResult viewResult = result as ViewResult;
            var model = Assert.IsAssignableFrom<List<Chat>>(viewResult.ViewData.Model);

            Assert.Equal(user, controller.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Assert.Equal(ChatAmount, model.Count());
        }

        //In onderstaande tests wordt gekeken of de gegevens uit de chat toeghangelijk zijn
        [Fact]
        public void TestInhoudTest(){
        DashboardController controller = getController(GetDatabase(),Pedagoog,"User1");
        var result = controller.Index("","",false);
        //var IndexActionResult =Assert.IsType<IActionResult>(result);
            
        ViewResult viewResult = result as ViewResult;
        var model = Assert.IsAssignableFrom<List<Chat>>(viewResult.ViewData.Model);

            Assert.Equal(model.First().Naam, "Chat1");
        }

        //Chat tests\\
        //In deze test testen wij of de juiste chat wordt meegegeven
        [Theory]
        [InlineData("User1",1,"Chat1")]
        [InlineData("User2",1,"Chat1")]
        [InlineData("User2",2,"Chat2")]
        [InlineData("User3",1,"Chat1")]
        [InlineData("User3",2,"Chat2")]
        public void TestChat1(string user,int ChatId,string expectedChatName){
            DashboardController controller = getController(GetDatabase(),Pedagoog,user);
            var result = controller.Chat(ChatId);

            ViewResult viewResult = result as ViewResult;
            var model = Assert.IsAssignableFrom<Chat>(viewResult.ViewData.Model);
            Assert.Equal(expectedChatName, model.Naam);
        }
        //Hier testen wij of de user wordt geredirect wordt naar de juiste pagina
        [Fact]
        public void TestChat2(){
            DashboardController controller = getController(GetDatabase(),Client,"User1");
            var result = controller.Chat(2);
            var ChatRedirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", ChatRedirect.ActionName);
        }

        //Create Room Tests\\
        //Met deze test wordt gekeken of de chat correct wordt aangemaakt
        [Fact]
        public void CreateRoomTests()
        {
            //Arrange
            var userId = "User1";
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();
            string expectedChatName = "TestChat";
            string expectedChatBeschrijving = "Deze chat is om te testen of de chat het goed doet";
            //Act
            var result = controller.CreateRoom(new Chat() { Naam = expectedChatName, Beschrijving = expectedChatBeschrijving });

            //met deze onderstaande methode pak je de laatste die is toegevoegd
            var chat = context.Chat.OrderByDescending(x => x.Id).First();

            //Assert
            Assert.Equal(expectedChatName, chat.Naam);
            Assert.Equal(expectedChatBeschrijving, chat.Beschrijving);
            Assert.Equal(User.UserName, chat.Users.First().User.UserName);
            Assert.Equal(UserRole.Admin, chat.Users.First().Role);
        }
        //Details Room\\
        [Theory]
        [InlineData("User1", 1, "Chat1")]
        [InlineData("User2", 1, "Chat1")]
        [InlineData("User2", 2, "Chat2")]
        public void TestDetails(string userId, int Chat, string expectedChatName)
        {
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();

            //Act
            var result = controller.Details(Chat);

            ViewResult viewResult = result as ViewResult;
            var model = Assert.IsAssignableFrom<Chat>(viewResult.ViewData.Model);

            //Assert          
            //Hiermee wordt gecheckt of de naam wel overeen komt met hetgene wat we vragen
            Assert.Equal(expectedChatName, model.Naam);
        }
        //In onderstaande test testen wij met een user die geen toegang heeft tot de gegevens
        [Fact]
        public void TestVerkeerdeRoom()
        {
            //arrange
            var userId = "User1";
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();

            //Act
            //de user zit niet in de chat 2
            var result = controller.Details(2);

            //Assert
            ViewResult viewResult = result as ViewResult;
            var ChatRedirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("NotAuthorized", ChatRedirect.ActionName);
        }
        //Delete Room\\
        [Theory]
        [InlineData("User1", 1, "Chat1")]
        [InlineData("User2", 1, "Chat1")]
        [InlineData("User2", 2, "Chat2")]
        public void DeleteRoomTest(string userId, int chatid, string chatName)
        {
            //arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();

            //Act
            //de user zit niet in de chat 2
            var result = controller.DeleteRoom(chatid, chatName);

            //Assert
            ViewResult viewResult = result as ViewResult;
            var Chat = context.Chat.Where(x => x.Id == chatid).SingleOrDefault();

            Assert.Null(Chat);
        }
        //Deze test of de user in de chat zit die hij verwijderd
        [Fact]
        public void DeleteRoomWhenNotInRoom()
        {
            //arrange
            var userId = "User1";
            var chatId = 2;
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();

            //Act
            //de user zit niet in de chat 2
            var result = controller.DeleteRoom(chatId, "Chat2");
            ViewResult viewResult = result as ViewResult;
            var ChatRedirect = Assert.IsType<RedirectToActionResult>(result);
            var Chat = context.Chat.Where(x => x.Id == chatId).SingleOrDefault();

            //Assert
            //Deze checkt of de chat niet verwijderd is
            Assert.NotNull(Chat);
            //Deze checkt of de test wordt doorgewezen
            Assert.Equal("NotAuthorized", ChatRedirect.ActionName);
        }
        //Hier komt een test dat je de chatname niet correct invult
        [Fact]
        public void DeleteRoomWrongNameTest()
        {
            //arrange
            var userId = "User1";
            var chatId = 1;
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();

            //Act
            //de user zit niet in de chat 2
            var result = controller.DeleteRoom(chatId, "Chat2");
            ViewResult viewResult = result as ViewResult;
            var ChatRedirect = Assert.IsType<RedirectToActionResult>(result);
            var Chat = context.Chat.Where(x => x.Id == chatId).SingleOrDefault();

            //Assert
            //Deze checkt of de chat niet verwijderd is
            Assert.NotNull(Chat);
            //Deze checkt of de test wordt doorgewezen
            Assert.Equal("DeleteRoom", ChatRedirect.ActionName);
        }
        //leave room test\\
        [Theory]
        [InlineData("User1", 1,"Chat1")]
        [InlineData("User2", 1,"Chat1")]
        [InlineData("User2", 2,"Chat2")]
        public void TestLeaveRoom(string userId, int chatId,string ChatRoomName)
        {
            //arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();

            //Act
            var result = controller.RemoveRoomFromList(ChatRoomName, chatId);

            //Assert
            ViewResult viewResult = result as ViewResult;
            var ChatUser = context.ChatUsers.Where(x => x.ChatId == chatId);
            var Chat = context.Chat.Where(x => x.Id == chatId).SingleOrDefault();

            //Dit is om te checken of de user is verwijderd
            Assert.Null(ChatUser.Where(x => x.UserId == userId).SingleOrDefault());
            //Dit is een check of de chat niet verwijderd wordt
            Assert.NotNull(Chat);
        }
        [Fact]
        public void LeaveRoomWithoutRoomAccess()
        {
            //arrange
            var userId = "User1";
            var chatId = 2;
            var ChatRoomName = "RoomId";
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();
            var expectedUsers = context.ChatUsers.Where(x=>x.ChatId==chatId).Count();
            //Act
            //de user zit niet in de chat 2
            var result = controller.RemoveRoomFromList(ChatRoomName, chatId);

            //Assert
            ViewResult viewResult = result as ViewResult;
            var ChatUser = context.ChatUsers.Where(x => x.ChatId == chatId);
            var Chat = context.Chat.Where(x => x.Id == chatId).SingleOrDefault();

            //Dit is om te checken of de user is verwijderd
            Assert.Null(ChatUser.Where(x => x.UserId == userId).SingleOrDefault());
            //Dit is een check of de chat niet verwijderd wordt
            Assert.NotNull(Chat);
            //Dit is een check om te kijken of er niet per ongeluk mensen zijn verwijderd van de chat
            Assert.Equal(expectedUsers, context.ChatUsers.Where(x => x.ChatId == chatId).Count());
        }
        //Edit test\\
        //In deze test wordt getest of de gegevens van de chat correct worden aangepast
        [Fact]
        public void TestEdit()
        {
            //arrange
            var userId = "User1";
            var chatId = 1;
            var ExpectedChatName = "Veranderde Chat";
            var expectedChatBeschrijving = "Dit is de nieuwe beschrijving voor de nieuwe chat";
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();
            var expectedUsers = context.ChatUsers.Where(x=>x.ChatId==chatId).Count();
            //Act
            var result = controller.Edit(chatId, ExpectedChatName, expectedChatBeschrijving);
            var chat = context.Chat.Where(x => x.Id == chatId).SingleOrDefault();
            //Assert
            //Deze checkt of de chat niet perongeluk verwijderd is
            Assert.NotNull(chat);
            //Deze checkt of de naam goed is geset
            Assert.Equal(ExpectedChatName, chat.Naam);
            //Deze test of de beschrijving goed is gedaan
            Assert.Equal(expectedChatBeschrijving, chat.Beschrijving);
        }
        //Deze test of je de chat niet kan veranderen als je er niet in zit
        [Fact]
        public void TestEditVanGroepWaarJeNietInzit()
        {
            //Arrange
            var userId = "User1";
            var chatId = 2;
            var expectedChatName = "Chat2";
            var expectedChatBeschrijving = "Dit is een chat applicatie2";
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,userId);
            var User = context.Users.Where(x=>x.Id==userId).Single();
            var expectedUsers = context.ChatUsers.Where(x=>x.ChatId==chatId).Count();

            //Act
            var result = controller.Edit(chatId, "Een gloednieuwe Chat", "Nieuwe Omschrijving van De Chat");
            var chat = context.Chat.Where(x => x.Id == chatId).SingleOrDefault();

            //Assert
            Assert.NotNull(chat);
            Assert.Equal(expectedChatName, chat.Naam);
            Assert.Equal(expectedChatBeschrijving, chat.Beschrijving);
        }

        //Join Chat\\
        [Theory]
        [InlineData("User4", 1)]
        [InlineData("User1", 2)]
        [InlineData("User4", 2)]
        public void JoinChatTest(string user, int chatOmTeJoinen)
        {
            //Arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,user);
            var User = context.Users.Where(x=>x.Id==user).Single();
            
            //Act
            var result = controller.JoinChat(chatOmTeJoinen);

            //Met onderstaande methode wordt de laatst toegevoegde ChatUser gepakt
            var chat = context.ChatUsers.Where(x => x.UserId == user);

            //Assert
            Assert.True(chat.Any(x => x.ChatId == chatOmTeJoinen));
        }
        [Fact]
        //Deze test zou een error geven als het er niet een maar meerdere zijn
        public void JoinChatWaarJeAlInzitTest()
        {
            //Arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,"User1");
            var User = context.Users.Where(x=>x.Id=="User1").Single();
            
            //Act
            var result = controller.JoinChat(1);

            //Met onderstaande methode wordt de laatst toegevoegde ChatUser gepakt
            var chat = context.ChatUsers.Where(x => x.UserId == "User1");

            //Assert
            Assert.NotNull(chat.Where(x => x.ChatId == 1).SingleOrDefault());
        }

        //Met onderstaande test testen we of de UserIsIn werkt
        [Theory]
        //Hiermee testen we of hij het doet
        [InlineData("User1", 1, true)]
        //hiermee testen wij of hij niet telkens de eerste pakt
        [InlineData("User2", 2, true)]
        //Hiermee testen we of hij ook een false terug geeft als de user er niet in zit
        [InlineData("User1", 2, false)]
        //Hiermee testen wij of hij niet telkens user1 pakt
        [InlineData("User4", 1, false)]
        public void TestUserIsIn(string user, int chat, bool expected)
        {
            //Arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context,Pedagoog,user);
            var User = context.Users.Where(x=>x.Id==user).Single();
            
            //Act
            var result = controller.UserIsIn(chat);
            //Assert
            Assert.Equal(expected, result);
        }
        [Theory]
        [InlineData("User1", "Chat", "Client")] //Dit is een client met pedagoog
        [InlineData("User2", "Index", "Client")] //Dit is een client zonder pedagoog
        [InlineData("User5", "Index", "Pedagoog")] //Dit is om te testen of hij leeg blijft. Anders geeft hij een error in de chatUser
        //Ik test hier of de redirect goed wordt gedaan.
        //Als de User geen prive chat heeft dan wordt deze toch geredirect naar de index pagina.
        public void TestGaNaarPriveChat(string userId, string expectedPage, string role)
        {
            //Arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context, role, userId);
            var User = context.Users.Where(x => x.Id == userId).Single();

            //Act
            var result = controller.GaNaarPriveChat();
            ViewResult viewResult = result as ViewResult;
            var ChatRedirect = Assert.IsType<RedirectToActionResult>(result);
            //Assert
            Assert.Equal(expectedPage, ChatRedirect.ActionName);
        }
        //In onderstaande test wordt de HeeftPriveChat gecheckt
        //Als dit het geval is dan krijgt de user een true geretured
        [Theory]
        [InlineData("User1", true, "Client")]
        [InlineData("User2", false, "Client")]
        [InlineData("User5", false, "Pedagoog")]
        public void TestheeftPriveChat(string userId, bool expectedBool, string role)
        {
            //arrange
            MijnContext context = GetDatabase();
            DashboardController controller = getController(context, role, userId);
            var User = context.Users.Where(x => x.Id == userId).Single();
            //act
            var result = controller.heeftPriveChat();
            //assert
            Assert.Equal(expectedBool, result);
        }
    }
}
