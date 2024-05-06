using Microsoft.AspNetCore.SignalR;
using printer_2.Services;
using Usb.Events;

namespace printer_2.Socket
{
    public class SocketHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"Client connected with connection ID: {connectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"Client disconnected with connection ID: {connectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessagea(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
