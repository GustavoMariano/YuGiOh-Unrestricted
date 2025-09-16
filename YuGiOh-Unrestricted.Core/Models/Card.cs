using YuGiOh_Unrestricted.Core.Models.Enums;

namespace YuGiOh_Unrestricted.Core.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public ECardType Type { get; set; }
    public int? Attack { get; set; }
    public int? Defense { get; set; }
    public int? Level { get; set; }
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";
}
