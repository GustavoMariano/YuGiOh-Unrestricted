namespace YuGiOh_Unrestricted.Models;

public class Match
{
    public List<Player> Players { get; set; } = new List<Player>();
    public int Turn { get; set; } = 1;
}