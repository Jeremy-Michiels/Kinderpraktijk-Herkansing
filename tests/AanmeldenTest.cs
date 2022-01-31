using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using src.Areas.Profile.Pages.Tabs;
using Xunit;

public class AanmeldenTest{
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
        public AanmeldenModel GetAanmeldenModel(MijnContext context,UserManager<srcUser> userManager){
            return new AanmeldenModel(userManager,GetStore(),null,null,null,context);
        }
        private async Task<bool> returnValue(){
            await Task.Delay(2);
            return true;
        }

        [Fact]
        //Deze methode test of het toevoegen van een role goed wordt gedaan
        public async void TestSetRole()
        {
            //Arrange
            MijnContext _context = GetDatabase();
            var mockUser = new Mock<UserManager<srcUser>>(GetStore(),null,null,null,null,null,null,null,null);
            mockUser.Setup(x=>x.AddToRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()));
            mockUser.Setup(x=>x.IsInRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>())).Returns(returnValue());
            AanmeldenModel controller = GetAanmeldenModel(_context,mockUser.Object);
            var user = _context.Users.First();
            //assert
            var result = await controller.SetRoleAsync(user);
            Assert.True(result);
            mockUser.Verify(x=>x.AddToRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()),Times.Once);
            mockUser.Verify(x=>x.IsInRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()),Times.Once);
        }
        
}