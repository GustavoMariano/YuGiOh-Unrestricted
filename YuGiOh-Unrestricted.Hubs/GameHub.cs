using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Core.Models;
using YuGiOh_Unrestricted.Infrastructure.Data;

namespace YuGiOh_Unrestricted.Hubs;

public class GameHub : Hub
{
    private static readonly object _lock = new();
    private static readonly Dictionary<string, RuntimeMatch> _matches = new();

    private readonly GameDbContext _db;

    public GameHub(GameDbContext db) { _db = db; }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("SetConnectionId", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public Task JoinGroup(string battleCode)
        => Groups.AddToGroupAsync(Context.ConnectionId, battleCode);

    public async Task JoinBattle(string battleCode, string playerName)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m))
            {
                m = new RuntimeMatch { BattleCode = battleCode };
                _matches[battleCode] = m;
            }

            if (!m.Players.Any(p => p.ConnectionId == Context.ConnectionId))
            {
                m.Players.Add(new RuntimePlayer
                {
                    ConnectionId = Context.ConnectionId,
                    Name = string.IsNullOrWhiteSpace(playerName) ? $"Player-{m.Players.Count + 1}" : playerName
                });
            }
        }
        await Broadcast(battleCode);
    }

    // Seleciona o deck do usuário e carrega como lista runtime; não valida login (prototipo)
    public async Task SelectDeck(string battleCode, Guid deckId)
    {
        var deck = await _db.Decks
            .Include(d => d.Cards).ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck == null) return;

        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m)) return;
            var me = m.GetByConn(Context.ConnectionId);
            if (me == null) return;

            me.Deck.Clear();
            foreach (var dc in deck.Cards)
            {
                for (int i = 0; i < dc.Count; i++)
                {
                    me.Deck.Add(new RuntimeCard
                    {
                        LibraryCardId = dc.CardId,
                        Name = dc.Card.Name,
                        Type = dc.Card.Type,
                        Attack = dc.Card.Attack,
                        Defense = dc.Card.Defense,
                        Level = dc.Card.Level,
                        Description = dc.Card.Description,
                        ImageUrl = dc.Card.ImageUrl,
                        IsFaceDown = true
                    });
                }
            }
        }
        await Broadcast(battleCode);
    }

    public async Task StartIfReady(string battleCode)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m)) return;
            if (m.Started) return;
            if (m.Players.Count != 2) return;
            if (m.Players.Any(p => p.Deck.Count < 1)) return;

            var rnd = new Random();
            foreach (var p in m.Players)
                p.Deck = p.Deck.OrderBy(_ => rnd.Next()).ToList();

            m.Started = true;
            m.Turn = 1;
        }
        await Broadcast(battleCode);
    }

    // Draw = 1 por clique (do próprio deck)
    public async Task DrawOne(string battleCode)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m)) return;
            var me = m.GetByConn(Context.ConnectionId);
            if (me == null || me.Deck.Count == 0) return;

            var c = me.Deck[0];
            me.Deck.RemoveAt(0);
            me.Hand.Add(c);
        }
        await Broadcast(battleCode);
    }

    // Sandbox ops
    public async Task RevealCard(string battleCode, string cardId, bool reveal) => await WithCard(battleCode, cardId, c => c.IsRevealed = reveal);
    public async Task SetFaceDown(string battleCode, string cardId, bool faceDown) => await WithCard(battleCode, cardId, c => c.IsFaceDown = faceDown);
    public async Task SetDefense(string battleCode, string cardId, bool defense) => await WithCard(battleCode, cardId, c => c.IsDefense = defense);

    public async Task MoveCard(string battleCode, string cardId, string targetConnId, ZoneType zone, int targetIndex = 0, DeckPosition deckPos = DeckPosition.Top)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m)) return;
            var owner = m.Players.FirstOrDefault(p => p.ConnectionId == targetConnId);
            if (owner == null) return;

            var card = RemoveFromWherever(m, cardId);
            if (card == null) return;

            AddTo(owner, zone, card, targetIndex, deckPos);
        }
        await Broadcast(battleCode);
    }

    public async Task ShuffleDeck(string battleCode, string ownerConnId)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m)) return;
            var owner = m.Players.FirstOrDefault(p => p.ConnectionId == ownerConnId);
            if (owner == null) return;
            var rnd = new Random();
            owner.Deck = owner.Deck.OrderBy(_ => rnd.Next()).ToList();
        }
        await Broadcast(battleCode);
    }

    // helpers
    private async Task WithCard(string battleCode, string cardId, Action<RuntimeCard> act)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(battleCode, out var m)) return;
            var card = FindCard(m, cardId);
            if (card == null) return;
            act(card);
        }
        await Broadcast(battleCode);
    }

    private RuntimeCard? FindCard(RuntimeMatch m, string cardId)
    {
        foreach (var p in m.Players)
        {
            var h = p.Hand.FirstOrDefault(c => c.Id == cardId);
            if (h != null) return h;

            var d = p.Deck.FirstOrDefault(c => c.Id == cardId);
            if (d != null) return d;

            var g = p.Graveyard.FirstOrDefault(c => c.Id == cardId);
            if (g != null) return g;

            foreach (var c in p.FieldMonsters) if (c?.Id == cardId) return c;
            foreach (var c in p.FieldSpellTraps) if (c?.Id == cardId) return c;
            if (p.FieldMagic?.Id == cardId) return p.FieldMagic;
        }
        return null;
    }

    private RuntimeCard? RemoveFromWherever(RuntimeMatch m, string cardId)
    {
        foreach (var p in m.Players)
        {
            var hi = p.Hand.FindIndex(c => c.Id == cardId);
            if (hi >= 0) { var c = p.Hand[hi]; p.Hand.RemoveAt(hi); return c; }

            var di = p.Deck.FindIndex(c => c.Id == cardId);
            if (di >= 0) { var c = p.Deck[di]; p.Deck.RemoveAt(di); return c; }

            var gi = p.Graveyard.FindIndex(c => c.Id == cardId);
            if (gi >= 0) { var c = p.Graveyard[gi]; p.Graveyard.RemoveAt(gi); return c; }

            for (int i = 0; i < p.FieldMonsters.Count; i++)
                if (p.FieldMonsters[i]?.Id == cardId) { var c = p.FieldMonsters[i]; p.FieldMonsters[i] = null; return c; }

            for (int i = 0; i < p.FieldSpellTraps.Count; i++)
                if (p.FieldSpellTraps[i]?.Id == cardId) { var c = p.FieldSpellTraps[i]; p.FieldSpellTraps[i] = null; return c; }

            if (p.FieldMagic?.Id == cardId) { var c = p.FieldMagic; p.FieldMagic = null; return c; }
        }
        return null;
    }

    private void AddTo(RuntimePlayer owner, ZoneType zone, RuntimeCard card, int idx, DeckPosition deckPos)
    {
        switch (zone)
        {
            case ZoneType.Hand:
                owner.Hand.Add(card); break;
            case ZoneType.Deck:
                if (deckPos == DeckPosition.Top) owner.Deck.Insert(0, card);
                else if (deckPos == DeckPosition.Bottom) owner.Deck.Add(card);
                else owner.Deck.Insert(owner.Deck.Count / 2, card);
                break;
            case ZoneType.Graveyard:
                owner.Graveyard.Add(card); break;
            case ZoneType.FieldMonster:
                idx = Math.Clamp(idx, 0, 4);
                owner.FieldMonsters[idx] = card; break;
            case ZoneType.FieldSpellTrap:
                idx = Math.Clamp(idx, 0, 4);
                owner.FieldSpellTraps[idx] = card; break;
            case ZoneType.FieldMagic:
                owner.FieldMagic = card; break;
        }
    }

    private Task Broadcast(string battleCode)
    {
        if (_matches.TryGetValue(battleCode, out var m))
            return Clients.Group(battleCode).SendAsync("UpdateMatch", m);
        return Task.CompletedTask;
    }
}
