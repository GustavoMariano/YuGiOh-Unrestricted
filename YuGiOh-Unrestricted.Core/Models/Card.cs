using YuGiOh_Unrestricted.Core.Models.Enums;

namespace YuGiOh_Unrestricted.Core.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CardId { get; set; }

    public string Name { get; set; } = string.Empty;
    public ECardType Type { get; set; }

    public int? Attack { get; set; }
    public int? Defense { get; set; }
    public int? Level { get; set; }

    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
