using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

public class MockDatabase{
        //Hieronder wordt een clean database aangemaakt.
        private string dbName;
        private readonly ITestOutputHelper output;
        //Dit is de methode die je oproept als je de database context nodig hebt voor het testen
        public MijnContext CreateContext(){
            MijnContext context = GetCleanContext(true);
            //hier wordt de database Geinitaliseerd.
            srcUser Alec = new srcUser(){Id="User1",UserName="Alecvanspr@gmail.com",SpecialistId="User5"};
            srcUser Jeremy = new srcUser(){Id="User2", UserName="Jeremy@gmail.com"};
            srcUser Claudio = new srcUser(){Id="User3",UserName="Claudio@gmail.com"};
            srcUser Bert = new srcUser(){Id="User4",UserName="BertVanAchternaam@gmail.com"};
            srcUser Emma = new srcUser(){Id="User5",UserName="EmmaDaBlat@Pedagoog.net"}; //Dit is een pedagoog
                Chat chat1= new Chat(){
                    Id=1,Naam="Chat1",Beschrijving="Dit is een chat applicatie", Messages= new List<Message>(){
                        new Message{ Naam="Alec",Text="Hoi",timestamp=DateTime.Now},
                        new Message{Naam="Alec",Text="Hoe gaat het",timestamp=DateTime.Now},
                        new Message{Naam="Claudio", Text="Goed met jou?",timestamp=DateTime.Now},
                        new Message{Naam="Jeremy", Text="Lekker",timestamp=DateTime.Now}
                    },Users = new List<ChatUser>(){
                        new ChatUser{UserId = Alec.Id ,User=Alec ,Role=UserRole.Admin},
                        new ChatUser{UserId = Jeremy.Id, User= Jeremy, Role= UserRole.Member},
                        new ChatUser{UserId= Claudio.Id, User=Claudio, Role= UserRole.Member}
                    }, type= ChatType.Room ,Onderwerp="ADD", Leeftijdscategorie="-14"};


                    Chat chat2= new Chat(){
                    Id=2,Naam="Chat2",Beschrijving="Dit is een chat applicatie2", Messages= new List<Message>(){
                        new Message{ Naam="Claudio",Text="GG",timestamp=DateTime.Now},
                        new Message{Naam="Jeremy",Text="EZ",timestamp=DateTime.Now},
                    },Users = new List<ChatUser>(){
                        new ChatUser{UserId = Claudio.Id ,User=Claudio ,Role=UserRole.Admin},
                        new ChatUser{UserId = Jeremy.Id, User = Jeremy, Role= UserRole.Member}
                    }, type= ChatType.Room, Onderwerp="Slaaptekort",Leeftijdscategorie="18-21"};

                    Chat chat3= new Chat(){
                    Id=3,Naam="Chat3",Beschrijving="Dit is een prive chat", Messages= new List<Message>(){
                        new Message{ Naam="Alec",Text="Is dit prive",timestamp=DateTime.Now},
                        new Message{Naam="Emma",Text="Ja",timestamp=DateTime.Now},
                    },Users = new List<ChatUser>(){
                        new ChatUser{UserId = Alec.Id ,User=Alec ,Role=UserRole.Member},
                        new ChatUser{UserId = Emma.Id, User = Emma, Role= UserRole.Admin}
                    }, type= ChatType.Private};
            context.Users.Add(Alec);
            context.Users.Add(Jeremy);
            context.Users.Add(Claudio);
            context.Users.Add(Bert);
            context.Users.Add(Emma);
            context.Chat.Add(chat1);
            context.Chat.Add(chat2);
            context.Chat.Add(chat3);
            context.SaveChanges();
            //Onderstaande code is voor de tests
            context.Meldingen.Add(new Melding(){Id=2,Titel="Melding2",Bericht="Hierin klaagt iemand over een bom ofzo",Datum=DateTime.Now});
            context.Meldingen.Add(new Melding(){Id=1,Titel="Melding1",Bericht="Dit is het eerste bericht om te testen of alles werkt",Datum=DateTime.Now});
            context.Meldingen.Add(new Melding(){Id=4,Titel="Melding4",Bericht="Deze klaagt dat gratis dingen niet kloppen",Datum=DateTime.Now});
            context.Meldingen.Add(new Melding(){Id=3,Titel="Melding3",Bericht="Een functie in de app werkt niet",Datum=DateTime.Now});
            context.SaveChanges();            
            context.Aanmeldingen.Add(new Aanmelding(){Id=1,Client=Alec, ClientId="User1",Pedagoog=Emma,PedagoogId="User5",AanmeldingDatum=DateTime.Now,IsAangemeld=true,IsAfgemeld=true});
            context.Aanmeldingen.Add(new Aanmelding(){Id=2,Client=Alec,ClientId="User1",Pedagoog=Emma,PedagoogId="User5",AanmeldingDatum=DateTime.Now,IsAangemeld=true,IsAfgemeld=false});
            context.Aanmeldingen.Add(new Aanmelding(){Id=3,Client=Claudio,ClientId="User2",Pedagoog=Emma,PedagoogId="User5",AanmeldingDatum=DateTime.Now,IsAangemeld=true,IsAfgemeld=false});
            context.Aanmeldingen.Add(new Aanmelding(){Id=4,Client=Jeremy,ClientId="User3",Pedagoog=Emma,PedagoogId="User5",AanmeldingDatum=DateTime.Now,IsAangemeld=false,IsAfgemeld=false});
            context.SaveChanges();
            return GetCleanContext(false);
        }
        //Dit zorgt ervoor dat je een schone context hebt.
        //Deze maakt eerst zodra nodig een schone context aan,
        //en daarna configureert hij de juiste options op de aangegeven naam
        private MijnContext GetCleanContext(bool clean){
            if (clean){
                dbName = Guid.NewGuid().ToString(); //Hier wordt een unieke naam aangemaakt
            }
                var options = new DbContextOptionsBuilder<MijnContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
                return new MijnContext(options);
        }
}