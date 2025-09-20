using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Core.Models;
using YuGiOh_Unrestricted.Infrastructure.Data;

namespace YuGiOh_Unrestricted.Hubs.Runtime;

public interface IMatchRuntime
{
    Task AttachConnectionAsync(string code, Guid userId, string name, string connectionId);
    Task SelectDeckAsync(string code, Guid userId, Guid deckId);
    Task SetReadyAsync(string code, Guid userId);
    Task DrawOneAsync(string code, Guid userId);
    Task RevealAsync(string code, string cardId, bool reveal);
    Task SetFaceDownAsync(string code, string cardId, bool faceDown);
    Task SetDefenseAsync(string code, string cardId, bool defense);
    Task MoveCardAsync(string code, string cardId, Guid targetUserId, ZoneType zone, int targetIndex, DeckPosition deckPos);
    Task ShuffleDeckAsync(string code, Guid userId);
    Task AdjustLifeAsync(string code, Guid targetUserId, int delta);
    Task TossCoinAsync(string code, Guid userId);
    Task RollDiceAsync(string code, Guid userId);
    RuntimeMatch GetOrCreate(string code);
}

public class MatchRuntime : IMatchRuntime
{
    private readonly object _lock = new();
    private readonly Dictionary<string, RuntimeMatch> _matches = new();
    private readonly IHubContext<GameHub> _hub;
    private readonly IServiceScopeFactory _scopes;

    public MatchRuntime(IHubContext<GameHub> hub, IServiceScopeFactory scopes)
    {
        _hub = hub;
        _scopes = scopes;
    }

    public RuntimeMatch GetOrCreate(string code)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m))
            {
                m = new RuntimeMatch { BattleCode = code };
                _matches[code] = m;
            }
            return m;
        }
    }

    public Task AttachConnectionAsync(string code, Guid userId, string name, string connectionId)
    {
        lock (_lock)
        {
            var m = GetOrCreate(code);
            var p = m.GetByUser(userId);
            if (p == null)
            {
                p = new RuntimePlayer { UserId = userId, Name = name, ConnectionId = connectionId };
                m.Players.Add(p);
            }
            else
            {
                p.ConnectionId = connectionId;
                p.Name = name;
            }
        }
        return Broadcast(code);
    }

    public async Task SelectDeckAsync(string code, Guid userId, Guid deckId)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        var deck = await db.Decks
            .Include(d => d.Cards)
            .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck == null) return;

        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return;
            var me = m.GetByUser(userId);
            if (me == null) return;

            me.IsReady = false;
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

        await Broadcast(code);
    }

    public Task SetReadyAsync(string code, Guid userId)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var match)) return Task.CompletedTask;
            var me = match.GetByUser(userId);
            if (me == null) return Task.CompletedTask;
            if (me.Deck.Count < 1) return Task.CompletedTask;

            me.IsReady = true;

            if (match.Players.Count == 2 && match.Players.All(p => p.IsReady))
            {
                var rnd = new Random();
                foreach (var p in match.Players)
                    p.Deck = p.Deck.OrderBy(_ => rnd.Next()).ToList();

                match.Started = true;
                match.Turn = 1;
            }
        }

        return Broadcast(code);
    }

    public Task DrawOneAsync(string code, Guid userId)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            if (!m.Started) return Task.CompletedTask;

            var me = m.GetByUser(userId);
            if (me == null || me.Deck.Count == 0) return Task.CompletedTask;

            var c = me.Deck[0];
            me.Deck.RemoveAt(0);
            c.IsFaceDown = false;
            me.Hand.Add(c);
        }

        return Broadcast(code);
    }

    public Task RevealAsync(string code, string cardId, bool reveal)
        => WithCard(code, cardId, c => c.IsRevealed = reveal);

    public Task SetFaceDownAsync(string code, string cardId, bool faceDown)
        => WithCard(code, cardId, c => c.IsFaceDown = faceDown);

    public Task SetDefenseAsync(string code, string cardId, bool defense)
        => WithCard(code, cardId, c => c.IsDefense = defense);

    public Task MoveCardAsync(string code, string cardId, Guid targetUserId, ZoneType zone, int targetIndex, DeckPosition deckPos)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            var owner = m.GetByUser(targetUserId) ?? m.Players.FirstOrDefault();
            if (owner == null) return Task.CompletedTask;

            var card = RemoveFromWherever(m, cardId);
            if (card == null) return Task.CompletedTask;

            AddTo(owner, zone, card, targetIndex, deckPos);
        }
        return Broadcast(code);
    }

    public Task ShuffleDeckAsync(string code, Guid userId)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            var owner = m.GetByUser(userId);
            if (owner == null) return Task.CompletedTask;
            var rnd = new Random();
            owner.Deck = owner.Deck.OrderBy(_ => rnd.Next()).ToList();
        }
        return Broadcast(code);
    }

    public Task AdjustLifeAsync(string code, Guid targetUserId, int delta)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            var p = m.GetByUser(targetUserId);
            if (p == null) return Task.CompletedTask;
            p.LifePoints += delta;
        }
        return Broadcast(code);
    }

    public Task TossCoinAsync(string code, Guid userId)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            var p = m.GetByUser(userId);
            if (p == null) return Task.CompletedTask;
            p.LastCoin = Random.Shared.Next(0, 2) == 0 ? "cara" : "coroa";
        }
        return Broadcast(code);
    }

    public Task RollDiceAsync(string code, Guid userId)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            var p = m.GetByUser(userId);
            if (p == null) return Task.CompletedTask;
            p.LastDice = Random.Shared.Next(1, 7).ToString();
        }
        return Broadcast(code);
    }

    private Task WithCard(string code, string cardId, Action<RuntimeCard> act)
    {
        lock (_lock)
        {
            if (!_matches.TryGetValue(code, out var m)) return Task.CompletedTask;
            var card = FindCard(m, cardId);
            if (card == null) return Task.CompletedTask;
            act(card);
        }
        return Broadcast(code);
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
                owner.Hand.Add(card);
                break;
            case ZoneType.Deck:
                if (deckPos == DeckPosition.Top) owner.Deck.Insert(0, card);
                else if (deckPos == DeckPosition.Bottom) owner.Deck.Add(card);
                else owner.Deck.Insert(owner.Deck.Count / 2, card);
                break;
            case ZoneType.Graveyard:
                owner.Graveyard.Add(card);
                break;
            case ZoneType.FieldMonster:
                idx = Math.Clamp(idx, 0, 4);
                owner.FieldMonsters[idx] = card;
                break;
            case ZoneType.FieldSpellTrap:
                idx = Math.Clamp(idx, 0, 4);
                owner.FieldSpellTraps[idx] = card;
                break;
            case ZoneType.FieldMagic:
                owner.FieldMagic = card;
                break;
        }
    }

    private Task Broadcast(string code)
    {
        if (_matches.TryGetValue(code, out var m))
            return _hub.Clients.Group(code).SendAsync("UpdateMatch", m);
        return Task.CompletedTask;
    }
}
