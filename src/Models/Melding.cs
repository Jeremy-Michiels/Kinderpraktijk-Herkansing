using System;
using System.ComponentModel.DataAnnotations;

public class Melding{
    public int Id{get;set;}
    [Required]
    [MaxLength(30)]
    public string Titel{get;set;}
    [Required]
    [MaxLength(300)]
    public string Bericht{get;set;}
    public DateTime Datum{get;set;}
}