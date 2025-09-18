using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Core.Models;
using YuGiOh_Unrestricted.Core.Models.Enums;
using YuGiOh_Unrestricted.Infrastructure.Data;
using YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

namespace YuGiOh_Unrestricted.Infrastructure.Services.Implementations;

public class CardService : ICardService
{
    private readonly GameDbContext _db;
    public CardService(GameDbContext db) => _db = db;

    public async Task<List<Card>> SearchAsync(ECardType? type, string? nameLike)
    {
        var q = _db.Cards.AsQueryable();
        if (type.HasValue) q = q.Where(c => c.Type == type.Value);
        if (!string.IsNullOrWhiteSpace(nameLike)) q = q.Where(c => c.Name.Contains(nameLike));
        return await q.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Card?> GetCardAsync(Guid cardId)
    {
        return await _db.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
    }
}
