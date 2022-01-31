using System;
using System.ComponentModel.DataAnnotations.Schema;

public class AanmeldingClient
    {
        public int Id { get; set; }

        [ForeignKey("srcUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string ClientId { get; set; }
        public srcUser Client {get;set;}
        public bool IsAangemeld { get; set; }
        public bool IsAfgemeld { get; set; }
        public DateTime Aanmelding { get; set; }
        public DateTime Afmelding { get; set; }
        public string srcUserId { get; set; }
        public srcUser Pedagoog{get;set;}
    }
