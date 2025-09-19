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
            me.Hand.Add(c);
        }

        return Broadcast(code);
    }

    private Task Broadcast(string code)
    {
        if (_matches.TryGetValue(code, out var m))
            return _hub.Clients.Group(code).SendAsync("UpdateMatch", m);
        return Task.CompletedTask;
    }
}
