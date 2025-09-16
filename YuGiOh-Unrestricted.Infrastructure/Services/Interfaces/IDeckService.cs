using YuGiOh_Unrestricted.Core.Models;

namespace YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

public interface IDeckService
{
    Task<List<Deck>> GetUserDecksAsync(Guid userId);
    Task<Deck> CreateDeckAsync(Guid userId, string name);
    Task<Deck?> GetDeckAsync(Guid deckId);
    Task RenameDeckAsync(Guid deckId, string newName);
    Task<bool> AddCopyAsync(Guid deckId, Guid cardId);
    Task RemoveCopyAsync(Guid deckId, Guid cardId);
}
