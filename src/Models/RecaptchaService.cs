using System;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class GooglereCAPTCHAService{
    private ReCAPTCHASettings _settings;
    public GooglereCAPTCHAService(IOptions<ReCAPTCHASettings> settings){
            _settings = settings.Value;
    }
    //Met deze methode wordt er een response opgevraagd van google ReCAPTCHA(De Site)
    public virtual async Task<GoogleResponse> VertifyResponse(string _Token){
        //Hiermee wordt de data gedefineerd die wordt meegestuurd aan de client.
        GooglereCAPTCHAData _MyData = new GooglereCAPTCHAData
        {
            response = _Token,
            secret = _settings.ReCAPTCHA_Sectret_Key
        };
        HttpClient client = new HttpClient();
        //Hier wordt een response gevraagd van de google ReCAPTCHA site.
        var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret="+_MyData.secret+"&response="+_MyData.response);

        //Hieronder wordt het resultaat wat binnenkomt als een Json omgezet naar een GoogleResponse
        var ReCAPTCHAResponse =  JsonConvert.DeserializeObject<GoogleResponse>(response);

        return ReCAPTCHAResponse;
    }
}
public class GooglereCAPTCHAData{
    public string response{get;set;} //Token
    public string secret{get;set;} //Dit is de secret key
}
//Dit is wat we terug krijgen gestuurd van google ReCAPTCHA
public class GoogleResponse{
    public bool success{get;set;}
    public double score{get;set;}
    public string action {get;set;}
    public DateTime challange_ts{get;set;}
    public string hostname{get;set;}
}