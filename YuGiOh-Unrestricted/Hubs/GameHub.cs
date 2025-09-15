using Microsoft.AspNetCore.SignalR;
using YuGiOh_Unrestricted.Models;

namespace YuGiOh_Unrestricted.Hubs
{
    public class GameHub : Hub
    {
        private static Match currentMatch = new Match();

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("SetConnectionId", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public async Task JoinMatch(string playerName)
        {
            if (!currentMatch.Players.Any(p => p.ConnectionId == Context.ConnectionId))
            {
                var player = new Player
                {
                    ConnectionId = Context.ConnectionId,
                    Name = playerName
                };

                // Inicializando deck com 5 cartas de teste
                for (int i = 1; i <= 5; i++)
                {
                    player.Deck.Add(new Card { Name = $"Card {i}" });
                }

                currentMatch.Players.Add(player);
            }

            await UpdateMatch();
        }

        public async Task DrawCard()
        {
            var player = currentMatch.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.Deck.Count == 0) return;

            var card = player.Deck[0];
            player.Deck.RemoveAt(0);
            player.Hand.Add(card);

            await UpdateMatch();
        }

        private async Task UpdateMatch()
        {
            await Clients.All.SendAsync("UpdateMatch", currentMatch);
        }
    }
}
