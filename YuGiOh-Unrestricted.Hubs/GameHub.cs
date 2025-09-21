using Microsoft.AspNetCore.SignalR;
using YuGiOh_Unrestricted.Hubs.Runtime;

namespace YuGiOh_Unrestricted.Hubs;

public class GameHub : Hub
{
    private readonly IMatchRuntime _runtime;

    public GameHub(IMatchRuntime runtime)
    {
        _runtime = runtime;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("SetConnectionId", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public Task JoinGroup(string battleCode)
        => Groups.AddToGroupAsync(Context.ConnectionId, battleCode);

    public async Task JoinBattle(string battleCode, string playerName, Guid userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, battleCode);
        await _runtime.AttachConnectionAsync(battleCode, userId, playerName, Context.ConnectionId);
        var match = _runtime.GetOrCreate(battleCode);
        await Clients.Caller.SendAsync("UpdateMatch", match);
    }


    public async Task RequestSync(string battleCode)
    {
        var m = _runtime.GetOrCreate(battleCode);
        await Clients.Caller.SendAsync("UpdateMatch", m);
    }
}
