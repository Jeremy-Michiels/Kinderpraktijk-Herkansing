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
using AutoMapper;

public class SpecialistTests{
        private MijnContext GetDatabase(){
            MockDatabase m = new MockDatabase();
            return m.CreateContext();
        }
        private async Task<bool> returnValue(){
            await Task.Delay(2);
            return true;
        }
        public IUserStore<srcUser> GetStore(){
            var mockStore = new Mock<IUserStore<srcUser>>(); 
            mockStore.Setup(x=>x.SetUserNameAsync(It.IsAny<srcUser>(),It.IsAny<String>(),It.IsAny<CancellationToken>()));
            return mockStore.Object;
        }
        public UserManager<srcUser> GetManager(){
            var mockUser = new Mock<UserManager<srcUser>>(GetStore(),null,null,null,null,null,null,null,null);
            mockUser.Setup(x=>x.AddToRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>()));
            mockUser.Setup(x=>x.IsInRoleAsync(It.IsAny<srcUser>(),It.IsAny<string>())).Returns(returnValue());
            return mockUser.Object;
        }
        //ClaimTypeId is denk een soort van static userId
        private ViewSpecialistModel getController(MijnContext context,string roleClaim,string ClaimTypeId){
            var mockImapper = new Mock<IMapper>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, roleClaim),
                new Claim(ClaimTypes.NameIdentifier,ClaimTypeId)
            }, "mock"));
            var controller = new ViewSpecialistModel(context,GetManager(),mockImapper.Object);
            controller.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext()
            {
            HttpContext = new DefaultHttpContext() { User = user }
        };
         return controller;
    }
    /* Dit is van een oudere methode. Die is overgezet en daarvan zijn de tests niet helemaal relevant meer
    //Dit is voor het testen van de GetUser
    [Theory]
    [InlineData("User1","Alecvanspr@gmail.com")]
    [InlineData("User2","Jeremy@gmail.com")]
    [InlineData("User3","Claudio@gmail.com")]
    public void TestGetUser(string ClaimTypeId, string ExpectedUsername){ 
        //Arrange
        MijnContext context = GetDatabase();
        SpecialistModel controller = getController(context,"Client",ClaimTypeId);
        //act
        var result = controller.getUser();
        //assert
        Assert.Equal(ExpectedUsername,result.UserName);
    }
    [Theory]
    [InlineData("Client","User1",true,false)] //Dit persoon heeft een pedagoog
    [InlineData("Client","User2", false,false)]//Dit persoon heeft geen pedagoog
    [InlineData("Pedagoog","User5",false,true)]//Dit persoon is een pedagoog
    //Dit test of de juiste booleans worden gereturned bij de specialistenPagina,
    //Deze booleans zorgen ervoor dat de user de juiste info te zien krijgen
    public void TestOnGet(string roleClaim, string ClaimTypeId,bool expectedHeeftPedagoog,bool expectedIsPedagoog){
        //Arrange
        MijnContext context = GetDatabase();
        SpecialistModel controller = getController(context,roleClaim,ClaimTypeId);
        //Act
        controller.OnGet();
        //Assert
        Assert.Equal(expectedHeeftPedagoog,controller.heeftPedagoog);
        Assert.Equal(expectedIsPedagoog, controller.IsPedagoog);
    }

    [Fact]
    //In onderstaande test wordt het connecteren met de pedagoog getest.
    public async void TestConnectWithPedagoog(){
        //Arrange
        var roleClaim = "Client";
        var ClaimTypeId = "User2";
        var SpecialistId = "User5";
        MijnContext context = GetDatabase();
        SpecialistModel controller = getController(context,roleClaim,ClaimTypeId);
        
        //Act
        await controller.OnPostConnectWithPedagoog(SpecialistId);

        //assert
        //hier wordt getest of hij wordt aangevult
        Assert.NotNull(context.Users.Find(ClaimTypeId).SpecialistId);
        //bij onderstaande test wordt gekeken of de juiste specialist wordt toegevoegd
        Assert.Equal(SpecialistId,context.Users.Find(ClaimTypeId).SpecialistId);
        Assert.True(controller.getSuccessvol());
        Assert.False(controller.getNietSuccessvol());
    }
    //In onderstaande test wordt getest of het toevoegen wel correct gebeurt.
    //en eentje waarvan het id niet klopt.
    [Fact]
    public async Task TestVerkeerdIngevoerdeCodeAsync()
    {
        var roleClaim = "Client";
        var ClaimTypeId = "User2";
        var SpecialistId = "NietKloppendId";
        MijnContext context = GetDatabase();
        SpecialistModel controller = getController(context,roleClaim,ClaimTypeId);
        
        //Act
        await controller.OnPostConnectWithPedagoog(SpecialistId);

        //assert
        //Hieronder wordt gekeken of het niet per ongeluk toch wel wordt toegevoegd
        Assert.Null(context.Users.Find(ClaimTypeId).SpecialistId);
        Assert.False(controller.getSuccessvol());
        Assert.True(controller.getNietSuccessvol());
    }
    */

}