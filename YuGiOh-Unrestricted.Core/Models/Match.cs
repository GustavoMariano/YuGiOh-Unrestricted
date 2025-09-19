namespace YuGiOh_Unrestricted.Core.Models;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = "";
    public List<MatchPlayer> Players { get; set; } = new();
    public int Turn { get; set; } = 1;
}
