using YuGiOh_Unrestricted.Core.Models;

namespace YuGiOh_Unrestricted.Core.Services;

public static class DeckRules
{
    public const int MaxCards = 50;
    public const int MaxCopiesPerCard = 3;
    public const int MaxDecksPerUser = 3;

    public static bool CanAddCard(Deck deck, Guid cardId)
    {
        var total = deck.Cards.Sum(c => c.Count);
        if (total >= MaxCards) return false;

        var dc = deck.Cards.FirstOrDefault(c => c.CardId == cardId);
        return dc == null || dc.Count < MaxCopiesPerCard;
    }

    public static void AddCard(Deck deck, Guid cardId)
    {
        if (!CanAddCard(deck, cardId)) return;
        var dc = deck.Cards.FirstOrDefault(c => c.CardId == cardId);
        if (dc == null) deck.Cards.Add(new DeckCard { CardId = cardId, Count = 1 });
        else dc.Count++;
    }

    public static void RemoveCard(Deck deck, Guid cardId)
    {
        var dc = deck.Cards.FirstOrDefault(c => c.CardId == cardId);
        if (dc == null) return;
        dc.Count--;
        if (dc.Count <= 0) deck.Cards.Remove(dc);
    }
}
