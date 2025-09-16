namespace YuGiOh_Unrestricted.Core.Models;

public class Player
{
    public string ConnectionId { get; set; } = "";
    public string Name { get; set; } = "";
    public int LifePoints { get; set; } = 8000;

    public List<Card> Deck { get; set; } = new();
    public List<Card> Hand { get; set; } = new();

    public List<Card?> FieldMonsters { get; set; } = new(new Card?[5]);
    public List<Card?> FieldSpellTraps { get; set; } = new(new Card?[5]);

    public Card? FieldMagic { get; set; } = null;
    public List<Card> Graveyard { get; set; } = new();
}
