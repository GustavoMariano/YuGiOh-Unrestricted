using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Core.Models;

namespace YuGiOh_Unrestricted.Infrastructure.Data;
public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Nickname)
            .IsUnique();

        modelBuilder.Entity<DeckCard>()
            .HasOne(dc => dc.Card)
            .WithMany()
            .HasForeignKey(dc => dc.CardId);

        // Seeding mínimo opcional (comente se não quiser)
        // modelBuilder.Entity<Card>().HasData(
        //     new Card { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name="Blue-Eyes White Dragon", Type=CardType.Monster, Attack=3000, Defense=2500, Level=8, Description="Legendary dragon.", ImageUrl="/images/blueeyes.jpg" }
        // );
    }
}
