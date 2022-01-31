using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub 
{
    //Deze methode send een message 
        public async Task SendMessage(string user, string message, string roomId)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        //Onderstaande methode zou ervoor moeten zorgen dat je in een group geconnect wordt
        public Task joinRoom(string roomId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        //Onderstaande methode zou ervoor moeten zorgen dat je uit de groep bent gegooid als het niet meer relevant is 
        public Task LeaveGroupAsync(string roomId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
}