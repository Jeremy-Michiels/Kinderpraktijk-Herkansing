using System;
using Xunit;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using src.Areas.Profile.Pages.Tabs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace tests
{
    public class GebruikersOverzichtTests
    {
        private MijnContext GetDatabase()
        {
            MockDatabase m = new MockDatabase();
            return m.CreateContext();
        }

        private GebruikersOverzichtModel getController(MijnContext context,string roleClaim,string ClaimTypeId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, roleClaim),
                new Claim(ClaimTypes.NameIdentifier,ClaimTypeId)
            }, "mock"));
            var controller = new GebruikersOverzichtModel(context);
            controller.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            return controller;
        }

        // Dit test of dat de lijst die wordt meegegeven bij de onget niet leeg is
        [Fact]
        public void TestOnGet()
        {
            //Arrange
            var roleClaim = "Admin";
            var ClaimTypeId = "User1";
            MijnContext context = GetDatabase();
            GebruikersOverzichtModel controller = getController(context, roleClaim, ClaimTypeId);

            //Act
            controller.OnGet();

            //Assert
            Assert.NotEmpty(controller.users);
        }

        // Dit test ofdat de gebruiker geblokkeerd wordt
        [Fact]
        public void TestBlokkeren()
        {
            //Arrange
            var roleClaim = "Admin";
            var ClaimTypeId = "User1";
            MijnContext context = GetDatabase();
            GebruikersOverzichtModel controller = getController(context, roleClaim, ClaimTypeId);
            srcUser user = context.Users.Where(p => p.Id == ClaimTypeId).FirstOrDefault();

            //Act
            controller.Blokkeren(user);
            var result = user.UserBlocked;

            //Assert
            Assert.True(result);
        }
    }
}