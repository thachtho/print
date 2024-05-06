

using Microsoft.AspNetCore.SignalR;
using printer_2.Socket;

namespace printer_2.Services
{
    public interface ISocketService
    {
        Task SocketToClient(string key, object responseVantayText);
    }
    public class SocketService : ISocketService
    {
        private readonly IHubContext<SocketHub> _hubContext;
        public SocketService(IHubContext<SocketHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SocketToClient(string key,object responseVantayText)
        {
            Console.WriteLine("Send ok");
            await _hubContext.Clients.All.SendAsync(key, responseVantayText);
        }
    }
}
