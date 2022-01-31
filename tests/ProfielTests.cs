using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using src.Areas.Profile.Pages.Tabs;
using Xunit;

public class ProfielTests{
        private MijnContext GetDatabase(){
            MockDatabase m = new MockDatabase();
            return m.CreateContext();
        }
        private ClaimsPrincipal getUser(string roleClaim,string ClaimTypeId){
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, roleClaim),
                new Claim(ClaimTypes.NameIdentifier,ClaimTypeId)
            }, "mock"));
            return user;
        }
        public IUserStore<srcUser> GetStore(){
            var mockStore = new Mock<IUserStore<srcUser>>(); 
            mockStore.Setup(x=>x.SetUserNameAsync(It.IsAny<srcUser>(),It.IsAny<String>(),It.IsAny<CancellationToken>()));
            return mockStore.Object;
        }
        public UserManager<srcUser> getUserManager(string userId){
            var mockUser = new Mock<UserManager<srcUser>>(GetStore(),null,null,null,null,null,null,null,null);
            mockUser.Setup(x=>x.AddToRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()));
            mockUser.Setup(x=>x.IsInRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>())).Returns(returnValue());
            mockUser.Setup(x=>x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            return mockUser.Object;
        }
        public ProfielModel getController(MijnContext context,string userId){
            return new ProfielModel(context,getUserManager(userId),null);
        }
        private async Task<bool> returnValue(){
            await Task.Delay(2);
            return true;
        }
        //In onderstaande test wordt de mehtode GetAanmeldingen getest
        [Fact]
        public void TestGetAanmeldingen(){
            //Arrange           
            var PedagoogId = "User5";
            MijnContext context = GetDatabase();
            ProfielModel controller = getController(context,PedagoogId);

            //Act
            var lijst = controller.GetAanmeldingen(PedagoogId);
            var eerste = lijst.FirstOrDefault();
            //Assert
            Assert.NotNull(lijst);
            Assert.NotNull(eerste);
        }
        //in onderstaande test wordt getest of het setten van de current user goed wordt gedaan
        [Theory]
        [InlineData("User1","Alec")]
        [InlineData("User3","Claudio")]
        [InlineData("User5","Emma")]
        public void TestSetCurrentUser(string UserId,string expectedName){
            //Arrange           
            MijnContext context = GetDatabase();
            ProfielModel controller = getController(context,UserId);
            //Act
            controller.SetCurrentUser(UserId); //Deze is null;
            //Assert
            Assert.NotNull(controller.CurrentUser);
        }
        //Met onderstaande test testen wij of de methode de gegevens successvol veranderd en opslaat in de database
        [Fact]
        public async Task OnPostMeldAan(){
            //Arrange           
            var pedagoogId = "User5";
            var UserId = "User3";
            MijnContext _context = GetDatabase();
            ProfielModel controller = getController(_context,pedagoogId);
            //Act
            await controller.OnPostMeldAan(UserId);
            var laatsteAanmelding = _context.Aanmeldingen.Where(x=>x.PedagoogId==pedagoogId).Where(x=>x.ClientId==UserId).OrderByDescending(x=>x.Id).FirstOrDefault();
            var CurrentUser = _context.Users.Where(x=>x.Id==UserId).FirstOrDefault();
            //Assert
            Assert.NotNull(laatsteAanmelding);
            Assert.NotNull(CurrentUser);
            Assert.True(laatsteAanmelding.IsAangemeld); 
            Assert.Equal(pedagoogId,CurrentUser.SpecialistId);
        }
        [Fact]
            public async Task OnPostMeldAf(){
            //Arrange           
            var pedagoogId = "User5";
            var UserId = "User1";
            MijnContext _context = GetDatabase();
            ProfielModel controller = getController(_context,pedagoogId);
            //Act
            await controller.OnPostMeldAf(UserId);
            var laatsteAanmelding = _context.Aanmeldingen.Where(x => x.PedagoogId == pedagoogId)
            .Where(x => x.ClientId == UserId)
            .Where(x => x.IsAangemeld)
            .Where(x => !x.IsAfgemeld)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();
            var CurrentUser = _context.Users.Where(x=>x.Id==UserId).FirstOrDefault();
            //Assert
            Assert.NotNull(laatsteAanmelding);
            Assert.NotNull(CurrentUser);
            Assert.Null(CurrentUser.SpecialistId);
            Assert.NotNull(laatsteAanmelding.AfmeldingDatum);
        }
        [Theory]
        [InlineData(false,false)]
        [InlineData(true,false)]
        [InlineData(false,true)]
        [InlineData(true,true)]
        public void TestOnPostFilter(bool expectedAan,bool expectedAf){
            //Arrange           
            var pedagoogId = "User5";
            var UserId = "User1";
            MijnContext _context = GetDatabase();
            ProfielModel controller = getController(_context,pedagoogId);
            //Act
            controller.OnPostFilter(expectedAan,expectedAf);
            //Assert
            Assert.Equal(expectedAan,controller.Aangemeld);
            Assert.Equal(expectedAf,controller.Afgemeld);
        }

    [Fact]
    //hierbij wordt getest of er een nieuwe groep wordt aangemaakt
    public async Task CreateNewGroupTestAsync(){
        //Arrange
        var roleClaim = "Client";
        var ClaimTypeId = "User2";
        var SpecialistId = "User5";
        MijnContext context = GetDatabase();
        ProfielModel controller = getController(context,SpecialistId);
        var user = context.Users.Where(x=>x.Id==ClaimTypeId).Single();
        var pedagoog =  context.Users.Where(x=>x.Id==SpecialistId).Single();

        //Hieronder staan de verwachte typen
        var expectedChatName ="Prive chat "+ user.LastName;
        var expectedChatUsers = 2;
        var expectedType = ChatType.Private;

        //act
        var result = await controller.CreateNewGroupAsync(user, pedagoog);
        var Chat =context.Chat.OrderByDescending(x=>x.Id).First();
        //assert
        //Deze checkt of het programma successvol is uitgevoerd
        Assert.True(result);
        //Dit checkt of de naam van klopt met de verwachte chat naam
        Assert.Equal(expectedChatName,Chat.Naam);
        
        //dit is om te checken of het juiste aantal in de chat zitten
        Assert.Equal(expectedChatUsers, Chat.Users.Count());

        //Dit is om te testen of zowel de pedagoog als de user in de chat zitten
        Assert.True(Chat.Users.Any(x=>x.UserId==user.Id));
        Assert.True(Chat.Users.Any(x=>x.UserId==pedagoog.Id));

        //Dit is om te checken of de kamer van het juiste type is
        Assert.Equal(expectedType,Chat.type);
    }
    //Hieronder testsen wij het verwijderen van een prive groep tussen een pedagoog en een user
    [Fact]
    public void DeletePrivateGroup(){
        //Arrange
        var ClaimTypeId = "User1";
        var SpecialistId = "User5";
        MijnContext context = GetDatabase();
        ProfielModel controller = getController(context,SpecialistId);
        var user = context.Users.Where(x=>x.Id==ClaimTypeId).Single();

        //Act
        var result = controller.DeletePrivateGroup(user);
        //Deze kijkt of er een prive chat is in de database.
        //tot zover is er maar een chat die prive is in de database
        var IsExist = context.Chat.Any(x=>x.type==ChatType.Private);
    
        //Assert
        Assert.True(result);
        Assert.False(IsExist);
    }
}