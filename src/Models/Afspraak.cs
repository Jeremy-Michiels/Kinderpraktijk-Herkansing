using System;
using System.ComponentModel.DataAnnotations;

public class Afspraak{
    public int Id{get; set;}
   [Required]
    public DateTime datum{get; set;}
    [Required]
    public DateTime Einddatum{get; set;}

    [Required]
    public string PedagoogId{get; set;}
    public srcUser Pedagoog{get; set;}
    
    [Required]
    public string ClientId{get; set;}
    public srcUser Client{get; set;}

    [Required]
    public string Beschrijving{get; set;}

}