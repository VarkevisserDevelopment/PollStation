using Microsoft.AspNetCore.SignalR;

namespace PollStation.Hubs
{
    public class PollHub : Hub
    {
        private static int OnlineUsers = 0;

        public override Task OnConnectedAsync()
        {
            OnlineUsers++;
            Clients.All.SendAsync("UpdateUsers", OnlineUsers);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            OnlineUsers--;
            Clients.All.SendAsync("UpdateUsers", OnlineUsers);
            return base.OnDisconnectedAsync(exception);
        }

        // Live update van stemmen voor alle clients
        public async Task BroadcastPollUpdate(object pollData)
        {
            await Clients.All.SendAsync("PollUpdated", pollData);
        }
    }
}