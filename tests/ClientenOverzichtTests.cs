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
    public class ClientenOverzichtTests
    {
        private MijnContext GetDatabase()
        {
            MockDatabase m = new MockDatabase();
            return m.CreateContext();
        }

        private ClientenOverzichtModel getController(MijnContext context,string roleClaim,string ClaimTypeId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, roleClaim),
                new Claim(ClaimTypes.NameIdentifier,ClaimTypeId)
            }, "mock"));
            var controller = new ClientenOverzichtModel(context);
            controller.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            return controller;
        }

        // Dit test of dat de lijst die wordt meegegeven bij de onget niet leeg is
        // Ook wordt er getest of dat alle users die in de lijst zitten de specialistId hebben van de huidige pedagoog
        [Fact]
        public void TestOnGet()
        {
            //Arrange
            var roleClaim = "Admin";
            var ClaimTypeId = "User5";
            MijnContext context = GetDatabase();
            ClientenOverzichtModel controller = getController(context, roleClaim, ClaimTypeId);

            //Act
            controller.OnGet();

            //Assert
            Assert.NotEmpty(controller.users);
            Assert.True(controller.users.All(p => p.SpecialistId == ClaimTypeId));
        }
    }
}