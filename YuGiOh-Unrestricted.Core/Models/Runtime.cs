using YuGiOh_Unrestricted.Core.Models.Enums;

namespace YuGiOh_Unrestricted.Core.Models;

public enum ZoneType { Hand, Deck, Graveyard, FieldMonster, FieldSpellTrap, FieldMagic }
public enum DeckPosition { Top, Middle, Bottom }

public class RuntimeCard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid LibraryCardId { get; set; }
    public string Name { get; set; } = "";
    public ECardType Type { get; set; }
    public int? Attack { get; set; }
    public int? Defense { get; set; }
    public int? Level { get; set; }
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";

    public bool IsFaceDown { get; set; }
    public bool IsDefense { get; set; }
    public bool IsRevealed { get; set; }
}

public class RuntimePlayer
{
    public string ConnectionId { get; set; } = "";
    public string Name { get; set; } = "";
    public int LifePoints { get; set; } = 8000;

    public List<RuntimeCard> Deck { get; set; } = new();
    public List<RuntimeCard> Hand { get; set; } = new();
    public List<RuntimeCard?> FieldMonsters { get; set; } = new(new RuntimeCard?[5]);
    public List<RuntimeCard?> FieldSpellTraps { get; set; } = new(new RuntimeCard?[5]);
    public RuntimeCard? FieldMagic { get; set; } = null;
    public List<RuntimeCard> Graveyard { get; set; } = new();
}

public class RuntimeMatch
{
    public string BattleCode { get; set; } = "";
    public List<RuntimePlayer> Players { get; set; } = new();
    public bool Started { get; set; } = false;
    public int Turn { get; set; } = 1;

    public RuntimePlayer? GetByConn(string connId) => Players.FirstOrDefault(p => p.ConnectionId == connId);
    public RuntimePlayer? GetOpponent(string connId) => Players.FirstOrDefault(p => p.ConnectionId != connId);
}
