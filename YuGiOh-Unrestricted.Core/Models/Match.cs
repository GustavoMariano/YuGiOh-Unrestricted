namespace YuGiOh_Unrestricted.Core.Models;

public class Match
{
    public List<Player> Players { get; set; } = new();
    public int Turn { get; set; } = 1;
}