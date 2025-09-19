namespace YuGiOh_Unrestricted.Core.Models;

public class MatchPlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = "";
}
