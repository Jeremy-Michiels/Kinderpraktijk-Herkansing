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
using src.Areas.Profile.Pages.Tabs;
using System.Threading;
using System.Threading.Tasks;

namespace tests
{
    public class AdminPanelTests
    {        
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
        public AddSpecialistModel GetController(MijnContext context,UserManager<srcUser> userManager){
            return new AddSpecialistModel(userManager,GetStore(),null,null,null,context);
        }
        private async Task<bool> returnValue(){
            await Task.Delay(2);
            return true;
        }
        //met onderstaande methode wordt getest of de Rol correct wordt toegewezen aan de user
        [Fact]
        public async Task TestSetRoleAsync(){
            MijnContext _context = GetDatabase();
            var mockUser = new Mock<UserManager<srcUser>>(GetStore(),null,null,null,null,null,null,null,null);
            mockUser.Setup(x=>x.AddToRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()));
            mockUser.Setup(x=>x.IsInRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>())).Returns(returnValue());
            AddSpecialistModel controller = GetController(_context,mockUser.Object);
            var user = _context.Users.First();
            //assert
            var result = await controller.SetRoleAsync(user);
            Assert.True(result);

            mockUser.Verify(x=>x.AddToRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()),Times.Once);
            mockUser.Verify(x=>x.IsInRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()),Times.Once);
        }
        //Met onderstaande test wordt getest of een user correct wordt aangemaakt.
        //Hier me wordt bedoeld dat alle ingevoerde velden kloppen
        [Fact]
        public void TestCreateUser()
        {
            //Arrange
            MijnContext _context = GetDatabase();
            var mockUser = new Mock<UserManager<srcUser>>(GetStore(),null,null,null,null,null,null,null,null);
            AddSpecialistModel controller = GetController(_context,mockUser.Object);
            var expectedFirstName = "Bassie";
            var expectedLastName=" van Adriaan";
            var expectedAge = DateTime.Now;
            var expectedSpecialism = "Clownen";
            var expectedDescription = "Hij is een clown";
            controller.Input = new AddSpecialistModel.InputModel3(){FirstName = expectedFirstName, LastName = expectedLastName,Age =expectedAge, Specialism= expectedSpecialism, Description= expectedDescription};
            //Act
            var User = controller.CreateUser();
            //assert
            Assert.Equal(expectedFirstName, User.FirstName);
            Assert.Equal(expectedLastName,User.LastName);
            Assert.Equal(expectedAge, User.Age);
            Assert.Equal(expectedSpecialism, User.Specialism);
            Assert.Equal(expectedDescription,User.Description);
        }
    }
}