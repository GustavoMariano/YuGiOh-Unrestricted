using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Core.Models;
using YuGiOh_Unrestricted.Core.Services;
using YuGiOh_Unrestricted.Infrastructure.Data;
using YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

namespace YuGiOh_Unrestricted.Infrastructure.Services.Implementations;

public class DeckService : IDeckService
{
    private readonly GameDbContext _db;
    public DeckService(GameDbContext db) { _db = db; }

    public async Task<List<Deck>> GetUserDecksAsync(Guid userId)
    {
        return await _db.Decks
            .Include(d => d.Cards).ThenInclude(dc => dc.Card)
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Deck> CreateDeckAsync(Guid userId, string name)
    {
        var count = await _db.Decks.CountAsync(d => d.UserId == userId);
        if (count >= DeckRules.MaxDecksPerUser)
            throw new InvalidOperationException("Maximum number of decks reached.");

        var d = new Deck { UserId = userId, Name = name };
        _db.Decks.Add(d);
        await _db.SaveChangesAsync();
        return d;
    }

    public async Task<Deck?> GetDeckAsync(Guid deckId)
    {
        return await _db.Decks
            .Include(d => d.Cards).ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task RenameDeckAsync(Guid deckId, string newName)
    {
        var d = await _db.Decks.FirstAsync(x => x.Id == deckId);
        d.Name = newName;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> AddCopyAsync(Guid deckId, Guid cardId)
    {
        var d = await GetDeckAsync(deckId) ?? throw new InvalidOperationException("Deck not found.");
        if (!DeckRules.CanAddCard(d, cardId)) return false;

        var dc = d.Cards.FirstOrDefault(x => x.CardId == cardId);
        if (dc == null) d.Cards.Add(new DeckCard { CardId = cardId, Count = 1 });
        else dc.Count++;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RemoveCopyAsync(Guid deckId, Guid cardId)
    {
        var d = await GetDeckAsync(deckId) ?? throw new InvalidOperationException("Deck not found.");

        var dc = d.Cards.FirstOrDefault(x => x.CardId == cardId);
        if (dc == null) return;

        dc.Count--;
        if (dc.Count <= 0) d.Cards.Remove(dc);
        await _db.SaveChangesAsync();
    }
}
