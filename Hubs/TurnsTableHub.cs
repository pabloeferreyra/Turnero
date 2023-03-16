using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Turnero.Hubs
{
    public class TurnsTableHub : Hub
    {

        public async Task UpdateTableDirected(string user, string message)
        {
            // Aquí puedes enviar el mensaje a los clientes conectados
            await Clients.User(user).SendAsync(message);
        }

        public async Task UpdateTable(string message)
        {
            // Aquí puedes enviar el mensaje a los clientes conectados
            await Clients.All.SendAsync(message);
        }
    }
}
