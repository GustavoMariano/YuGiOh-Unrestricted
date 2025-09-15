using YuGiOh_Unrestricted.Models.Enums;

namespace YuGiOh_Unrestricted.Models;

public class Card
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Level { get; set; }
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsDefense { get; set; } = false;
    public bool IsFaceDown { get; set; } = false;
}
