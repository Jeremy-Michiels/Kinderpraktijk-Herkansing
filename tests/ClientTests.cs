using Xunit;

public class ClientTests{
    private MijnContext GetContext(){
        MockDatabase m = new MockDatabase();
        return m.CreateContext();
    }
    //Testen of de methode CreateRoom wordt gemaakt
    [Fact]
    public void TestGetClients(){

    }
    //Testen of de method GetClients werkt

    //Testen of de methode Zoeken op werkt

    //Testen of de methode index in zijn geheel werkt
}