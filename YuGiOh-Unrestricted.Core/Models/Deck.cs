namespace YuGiOh_Unrestricted.Core.Models;

public class Deck
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Name { get; set; } = "New Deck";

    public ICollection<DeckCard> Cards { get; set; } = new List<DeckCard>();
}

public class DeckCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeckId { get; set; }
    public Deck Deck { get; set; } = default!;
    public Guid CardId { get; set; }
    public Card Card { get; set; } = default!;
    public int Count { get; set; } = 1;
    public int Position { get; set; }
}
