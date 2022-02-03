using System;

public class Aanmelding{
    public int Id{get;set;}
    public string ClientId{get;set;}
    public srcUser Client {get;set;}
    public string PedagoogId{get;set;}
    public srcUser Pedagoog {get;set;}
    public string AssistentId{get; set;}
    public srcUser Assistent{get; set;}
    public bool IsAangemeld{get;set;}
    public bool IsAfgemeld{get;set;}
    public DateTime AanmeldingDatum { get; set; }
   public DateTime AfmeldingDatum { get; set; }
}