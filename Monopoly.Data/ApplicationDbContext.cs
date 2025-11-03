using Microsoft.EntityFrameworkCore;
using Monopoly.Core.Entities;

namespace Monopoly.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }
    public DbSet<GameBoard> GameBoards { get; set; }
    public DbSet<BoardCell> BoardCells { get; set; }
    public DbSet<PlayerState> PlayerStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PlayerCount).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CurrentTurnUser)
                .WithMany()
                .HasForeignKey(e => e.CurrentTurnUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GamePlayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlayerOrder).IsRequired();
            
            // Configure relationship with Game
            entity.HasOne(e => e.Game)
                .WithMany(g => g.GamePlayers)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure relationship with User
            entity.HasOne(e => e.User)
                .WithMany(u => u.GamePlayers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Ensure a user can only join a game once
            entity.HasIndex(e => new { e.GameId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<GameBoard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Configure one-to-one relationship with Game
            entity.HasOne(e => e.Game)
                .WithOne(g => g.GameBoard)
                .HasForeignKey<GameBoard>(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Ensure one game can only have one board
            entity.HasIndex(e => e.GameId).IsUnique();
        });

        modelBuilder.Entity<BoardCell>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Position).IsRequired();
            entity.Property(e => e.CellType).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            
            // Configure relationship with GameBoard
            entity.HasOne(e => e.GameBoard)
                .WithMany(gb => gb.BoardCells)
                .HasForeignKey(e => e.GameBoardId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Ensure unique position per board
            entity.HasIndex(e => new { e.GameBoardId, e.Position }).IsUnique();
        });
    }
}
