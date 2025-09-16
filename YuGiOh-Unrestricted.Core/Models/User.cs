namespace YuGiOh_Unrestricted.Core.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nickname { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public ICollection<Deck> Decks { get; set; } = new List<Deck>();
}