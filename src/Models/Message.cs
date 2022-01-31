using System;

public class Message{
    public int Id{get;set;} //doordat het in een database moet is hier ook een ID voor nodig
    public string Naam{get;set;} //De naam an de user
    public string Text{get;set;} //het bericht
    public DateTime timestamp {get;set;} //De tijd waarop die is geplaatst

    public int ChatId{get;set;}
    public Chat Chat{get;set;}
}
