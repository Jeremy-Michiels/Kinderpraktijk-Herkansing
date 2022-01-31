using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Chat{
    public int Id{get;set;}
    public string Onderwerp{get; set;}
    
    [Required]
    [MinLength(5)]
    [MaxLength(30)]
    public string Naam{get;set;}

    [Required]
    [MinLength(5)]
    [MaxLength(250)]
    public string Beschrijving{get;set;}
    public string Leeftijdscategorie{get; set;}
    public ICollection<Message> Messages{get;set;} //Dit zijn de berichten die zijn geplaatst
    public ICollection<ChatUser> Users{get;set;} //Dit zijn de Users voor de app
    public ChatType type {get;set;} //Dit is het sype voor de app
}
