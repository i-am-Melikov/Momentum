using Microsoft.AspNetCore.SignalR;

namespace Momentum.Hubs.SignalRHubs
{
    public class ContactHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
