namespace YuGiOh_Unrestricted.Models;

public class Player
{
    public string ConnectionId { get; set; } = "";
    public string Name { get; set; } = "";
    public int LifePoints { get; set; } = 8000;

    public List<Card> Deck { get; set; } = new List<Card>();
    public List<Card> Hand { get; set; } = new List<Card>();
    public List<Card> Field { get; set; } = new List<Card>();
    public List<Card> SpellTrap { get; set; } = new List<Card>();
    public Card? FieldMagic { get; set; } = null;
    public List<Card> Graveyard { get; set; } = new List<Card>();
}
