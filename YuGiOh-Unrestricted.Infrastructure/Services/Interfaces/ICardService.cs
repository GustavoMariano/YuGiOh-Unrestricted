using YuGiOh_Unrestricted.Core.Models;
using YuGiOh_Unrestricted.Core.Models.Enums;

namespace YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

public interface ICardService
{
    Task<List<Card>> SearchAsync(ECardType? type, string? nameLike);
    Task<Card?> GetCardAsync(Guid cardId);
}
