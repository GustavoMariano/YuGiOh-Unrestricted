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
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();

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

        modelBuilder.Entity<Match>()
            .HasMany(m => m.Players)
            .WithOne()
            .HasForeignKey(p => p.MatchId);
    }
}
