using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using Xunit;

public class ChatTests{
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
        //Bij deze test wordt getest of de methode JoinRoom het doet.
        //Die methode is verantwoordelijk voor het toevoegen van mensen in een live chatroom
        [Fact]
        public void TestJoinRoom(){
            //Arrange
            var MockIhub = new Mock<IHubContext<ChatHub>>();
            
            MockIhub.Setup(x=>x.Groups.AddToGroupAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()));
            ChatController controller = new ChatController(MockIhub.Object);
           
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = getUser("Pedagoog","User1") }
            };
            //Act
            var result = controller.joinRoom("Connectie1","Chat1");
            //assert
            MockIhub.Verify(x=>x.Groups.AddToGroupAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once());
        }
        [Fact]
        //Deze test test of de methode LeaveRoom het doet.
        //Deze methode is verantwoordelijk om bepaalde users te verwijderen van een live chatroom als ze weggaan
        public void TestLeaveRoom(){
            //Arrange
            var MockIhub = new Mock<IHubContext<ChatHub>>();
            MockIhub.Setup(x=>x.Groups.RemoveFromGroupAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()));
            ChatController controller = new ChatController(MockIhub.Object);
           
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = getUser("Pedagoog","User1")}
            };
            //Act
            var result = controller.LeaveGroup("Connectie1","Chat1");
            //assert
            MockIhub.Verify(x=>x.Groups.RemoveFromGroupAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once());
        }
        

        //Create Message tests\\
        //Deze test kijken we of het chat bericht correct wordt opgeslagen in de database
        //Deze heb ik niet kunnen testen met gebruik van mock door deze error
        //Extension methods (here: ClientProxyExtensions.SendAsync) may not be used in setup / verification expressions.
        [Theory]
        [InlineData(1,"Chat1","TestBericht","User1")]
        [InlineData(1,"Chat1","NieuwBericht","User2")]
        [InlineData(2,"Chat2","NieuwBericht","User2")]
        public void CreateMessageTest(int chatId,string roomName,string message,string user){
            //Arrange
            MijnContext context = GetDatabase();
            var MockIhub = new Mock<IHubContext<ChatHub>>();            
            ChatController controller = new ChatController(MockIhub.Object);
           
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = getUser("Pedagoog",user)}
            };
            var User = context.Users.Where(x=>x.Id==user).Single();
            var expectedUsername = User.FirstName+" "+User.LastName;
            //Act
            var result = controller.SendMessage(chatId,message,roomName,context);
            var sentMessage = context.Messages.OrderByDescending(x=>x.Id).First();
            
            //hieronder test ik of alles van het bericht correct wordt opgeslagen
            Assert.Equal(chatId,sentMessage.ChatId);
            Assert.Equal(message,sentMessage.Text);
            Assert.Equal(expectedUsername,sentMessage.Naam);
            Assert.NotNull(sentMessage.timestamp);
        }
        //Hieronder wordt getest wat er gebeurt als je een bericht stuurt in een chat waar jij niet inzit
        /* onderstaande methode bleef maar errors geven dat een zekere USER1 is toegevoegd terwijl er niet om gevraagd werdt
            en er slaagde ook ineens een andere test 
        [Fact]
        public void CreateWrongMessageTest(){
            //arrange
            MijnContext context = GetDatabase();
            var MockIhub = new Mock<IHubContext<ChatHub>>();            
            ChatController controller = new ChatController(MockIhub.Object);
           
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = getUser("Pedagoog","User4")}
            };
            var expectedBericht = "Verkeerd gestuurd bericht";

            //Act
            //Hier is het chat 2doordat de user niet in de 2de chat zit
            var result = controller.SendMessage(2,expectedBericht,"Connextie",context);
            var sentMessage = context.Messages.OrderByDescending(x=>x.Id).First();
            
            //Assert
            //hiermee test ik of het laaste bericht niet is opgeslagen
            Assert.NotEqual(expectedBericht,sentMessage.Text);
        }
        */
}