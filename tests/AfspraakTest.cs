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
using src.Controllers;
using System.Threading.Tasks;


namespace Prime.UnitTests.Services
{
    public class PrimeService_IsPrimeShould
    {
        private MijnContext GetDatabase()
        {
            MockDatabase m = new MockDatabase();
            return m.CreateContext();
        }
        private AfspraakController getController(MijnContext context, string roleClaim, string ClaimTypeId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, roleClaim),
                new Claim(ClaimTypes.NameIdentifier,ClaimTypeId)
            }, "mock"));

            var controller = new AfspraakController(context);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
            return controller;
        }
        private string Pedagoog = "Pedagoog";
        private string Assistent = "Assistent";

        private DateTime datum1 = new DateTime(2021, 2, 3, 20, 14, 00);
        private DateTime EindDatum1 = new DateTime(2021, 2, 3, 21, 14, 00);
        
        private DateTime datum2 = new DateTime(2021, 2, 3, 22, 14, 00);
        private DateTime EindDatum2 = new DateTime(2021, 2, 3, 23, 14, 00);

        private DateTime datum3 = new DateTime(2021, 2, 3, 20, 20, 00);
        private DateTime EindDatum3 = new DateTime(2021, 2, 3, 20, 40, 00);

        private string clientid = "String1";

        private string pedagoogid = "string2";

        private Afspraak MaakAfspraak1(){
            return new Afspraak(){datum = datum1, Einddatum = EindDatum1, ClientId = clientid, PedagoogId = pedagoogid, Beschrijving="" };
        }
        private Afspraak MaakAfspraak2(){
            return new Afspraak(){datum = datum2, Einddatum = EindDatum2, ClientId = clientid, PedagoogId = pedagoogid, Beschrijving="" };
        }
        private Afspraak MaakAfspraak3(){
            return new Afspraak(){datum = datum3, Einddatum = EindDatum3, ClientId = clientid, PedagoogId = pedagoogid, Beschrijving="" };
        }

        
        [Fact]
        public void PostAfspraakTest(){
            MijnContext context = GetDatabase();
            AfspraakController controller= getController(context, Pedagoog, "User1");
            var x = MaakAfspraak1();
            var y = MaakAfspraak2();

            var post1 = controller.PostAfspraak(x);
            var post2 = controller.PostAfspraak(y);

            var result = Assert.IsType<Task<ActionResult<Afspraak>>>(post1);
            var result2 = Assert.IsType<Task<ActionResult<Afspraak>>>(post2);

            Assert.NotEmpty(context.Afspraken);
            Assert.Equal(context.Afspraken.Count(), 2);
            
        }
        [Fact]
        public void PostAfspraakTegelijk(){
            //Het verschil met vorige test is dat hier 2 afspraken worden gemaakt die tegelijkertijd zijn
            MijnContext context = GetDatabase();
            AfspraakController controller= getController(context, Pedagoog, "User1");
            var x = MaakAfspraak1();
            var y = MaakAfspraak3();

            var post1 = controller.PostAfspraak(x);
            var post2 = controller.PostAfspraak(y);

            var result = Assert.IsType<Task<ActionResult<Afspraak>>>(post1);
            var result2 = Assert.IsType<Task<ActionResult<Afspraak>>>(post2);

            Assert.NotEmpty(context.Afspraken);
            Assert.Equal(context.Afspraken.Count(), 1);
        }


    
        [Fact]
        public void getAfsprakenTest()
        {
            AfspraakController controller = getController(GetDatabase(), Pedagoog, "User1");
            var get = controller.GetAfspraken();
            
            var result = Assert.IsType<Task<ActionResult<IEnumerable<Afspraak>>>>(get);
        }

        [Fact]
        public void DeleteAfsprakenTest()
        {
            MijnContext context = GetDatabase();
            AfspraakController controller= getController(context, Pedagoog, "User1");
            var x = MaakAfspraak1();
            var y = MaakAfspraak2();

            var post1 = controller.PostAfspraak(x);
            var post2 = controller.PostAfspraak(y);
            
            var delpost1 = controller.DeleteAfspraak(post1.Id);
            var result = Assert.IsType<Task<IActionResult>>(delpost1);

            Assert.NotEmpty(context.Afspraken);
            Assert.Equal(context.Afspraken.Count(), 1);
        }

        [Fact]
        public void TestInputNaDelete()
        {
            //test of na het deleten van een afspraak, een andere afspraak die op hetzelfde tijdstip zou plaatsvinden, nu wel kan plaatsvinden
            MijnContext context = GetDatabase();
            AfspraakController controller= getController(context, Pedagoog, "User1");
            var x = MaakAfspraak1();
            var y = MaakAfspraak3();

            var post1 = controller.PostAfspraak(x);
            var post2 = controller.PostAfspraak(y);

            Assert.NotEmpty(context.Afspraken);
            
            
            var delpost1 = controller.DeleteAfspraak(post1.Id);
            Assert.Equal(context.Afspraken.Count(), 0);
            var result = Assert.IsType<Task<IActionResult>>(delpost1);

            var post2opnieuw = controller.PostAfspraak(y);
            var result2 = Assert.IsType<Task<ActionResult<Afspraak>>>(post2opnieuw);

            Assert.NotEmpty(context.Afspraken);
            Assert.Equal(context.Afspraken.Count(), 1);


        }
    }
}