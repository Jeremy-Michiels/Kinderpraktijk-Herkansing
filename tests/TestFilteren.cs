using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class filterenEnSorteren{
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
        //Met onderstaande test wordt getest of de GetChats methode goed werkt
        //Hiermee wordt verwacht dat er een list met alle Room chats wordt gegeven
        [Fact]
        public void TestGetChats(){
            //arrange
            var userId = "User1";
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var expectedCount = 1;
            
            //act
            var result = controller.GetChats();
            var aantalChats = result.Count();

            //assert
            Assert.Equal(expectedCount, aantalChats);
        }
        //Met onderstaande methode wordt getest of het filteren op onderwerp goed wordt uitgevoerd
        //Hierbij testen wij of er goed gefilterd wordt op het type ONDERWERP
        //Dit doen wij door met verschillende parameters te testen
        [Theory]
        [InlineData("User1","ADD",1)]
        [InlineData("User1","Slaaptekort",0)]
        [InlineData("User1","ADHD",0)]
        [InlineData("User2","ADD",1)]
        [InlineData("User2","Slaaptekort",1)]
        [InlineData("User2","ADHD",0)]
        [InlineData("User4","ADD",0)]
        [InlineData("User4","Slaaptekort",0)]
        [InlineData("User4","ADHD",0)]
        public void TestOnderwerp(string userId,string onderwerp,int expected){
            //arrange
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat); 
            
            //Act
            var result = (controller.Onderwerp(lijst,onderwerp));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //Bij onderstaande test testen wij of hij de list teruggeeft als je geen parameters meegeeft
        [Fact]
        public void TestLeegOnderwerp(){
            //arrange
            var userId = "User2";
            var expected = 2;
            var onderwerp = "";
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat); 
            
            //Act
            var result = (controller.Onderwerp(lijst,onderwerp));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //Onderstaande test wordt getest of hij het juiste aantal teruggeeft voor de leeftijdscatagorie
        [Theory]
        [InlineData("User1","-14",1)]
        [InlineData("User1","18-21",0)]
        [InlineData("User2","-14",1)]
        [InlineData("User2","18-21",1)]
        public void TestLeeftijdsCatagorie(string userId,string leeftijdscatagorie,int expected){
            //arrange
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat); 
            
            //Act
            var result = (controller.LeeftijdsCatagorie(lijst,leeftijdscatagorie));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //Met onderstaande test testen de methode wel een lijst terug geeft als je geen leeftijdscatagorie meegeeft
        [Fact]
        public void TestLeeftijdsCatagorieLeeg(){
            //arrange
            var userId = "User2";
            var expected = 2;
            var LeeftijdsCatagorie = "";
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat); 
            
            //Act
            var result = (controller.LeeftijdsCatagorie(lijst,LeeftijdsCatagorie));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //in onderstaande test testen wij of er correct gefilterd wordt als er op een bepaalde titel gezocht wordt
        [Theory]
        //Bij deze Chat testen wij een deel van de titel
        //Met Chat1 vragen wij de complete titel
        //Met Chat2 vragen wij een deel van de titel
        [InlineData("User1","Chat",1)]
        [InlineData("User1","Chat1",1)]
        [InlineData("User1","Chat2",0)]
        [InlineData("User2","Chat",2)]
        [InlineData("User2","Chat1",1)]
        [InlineData("User2","Chat2",1)]
        public void TestFilterTitel(string userId,string titel,int expected){
            //arrange
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat).Where(x=>x.type==ChatType.Room); 
            
            //Act
            var result = (controller.FilterTitel(lijst,titel));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //Met onderstaande test wordt gekeken of een titel leeg gelaten kan worden en dan nog steeds het juiste terug geeft
        [Fact]
        public void TestFilterTitelLeeg(){
            //arrange
            var userId = "User2";
            var expected = 2;
            var Titel = "";
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat); 
            
            //Act
            var result = (controller.FilterTitel(lijst,Titel));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //In onderstaande test testen wij of wij gedeeltes van de beschrijving kunnen vragen en kijken of hij dan het juiste terug geeft
        [Theory]
        //Bij deze A testen wij een deel van de titel
        //Met Chat1 vragen wij de complete titel
        //Met Chat2 vragen wij een deel van de titel
        [InlineData("User1","a",1)]
        [InlineData("User1","2",0)]
        [InlineData("User1","q",0)]
        [InlineData("User2","a",2)]
        [InlineData("User2","2",1)]
        [InlineData("User2","q",0)]
        public void TestFilterBeschrijving(string userId,string Beschrijving,int expected){
            //arrange
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat).Where(x=>x.type==ChatType.Room); 
            
            //Act
            var result = (controller.FilterBeschrijving(lijst,Beschrijving));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
        //Met onderstaande test wordt er gekeken of het filter leeg gelaten kan worden en dan nogsteeds een lijst terug geeft
        [Fact]
        public void TestFilterBeschrijvingLeeg(){
            //arrange
            var userId = "User2";
            var expected = 2;
            var Beschrijving = "";
            MijnContext _context = GetDatabase();
            DashboardController controller = getController(_context,"Client",userId);
            var lijst = _context.ChatUsers.Include(x=>x.chat).Where(x=>x.UserId==userId).Select(x=>x.chat); 
            
            //Act
            var result = (controller.FilterBeschrijving(lijst,Beschrijving));
            var resultCount = result.Count();

            Assert.Equal(expected,resultCount);
        }
}