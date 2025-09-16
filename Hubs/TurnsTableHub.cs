namespace Turnero.Hubs;

public class TurnsTableHub : Hub
{

    public async Task UpdateTableDirected(string user, string message, string date)
    {
        await Clients.User(user).SendAsync(message, date);
    }

    public async Task UpdateTable(string message)
    {
        await Clients.All.SendAsync(message);
    }
}
