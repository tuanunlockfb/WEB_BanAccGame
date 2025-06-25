using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<GameAccount> GameAccounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data for GameCategories
            modelBuilder.Entity<GameCategory>().HasData(
                new GameCategory { Id = 1, Name = "MMORPG", Description = "Massively Multiplayer Online Role-Playing Game", IconUrl = "🎮" },
                new GameCategory { Id = 2, Name = "FPS", Description = "First-Person Shooter", IconUrl = "🔫" },
                new GameCategory { Id = 3, Name = "MOBA", Description = "Multiplayer Online Battle Arena", IconUrl = "⚔️" },
                new GameCategory { Id = 4, Name = "Battle Royale", Description = "Last Man Standing Games", IconUrl = "🏆" },
                new GameCategory { Id = 5, Name = "Survival", Description = "Survival Games", IconUrl = "🏕️" }
            );

            // Configure decimal precision for MySQL
            modelBuilder.Entity<GameAccount>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameCategory>()
                .HasMany(c => c.GameAccounts)
                .WithOne(g => g.Category)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Review relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.GameAccount)
                .WithMany()
                .HasForeignKey(r => r.GameAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Wishlist relationships
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.GameAccount)
                .WithMany()
                .HasForeignKey(w => w.GameAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint for Wishlist (one user can only wishlist a product once)
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.GameAccountId })
                .IsUnique();

            // Fix for MySQL - set string columns to appropriate length
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Id)
                .HasMaxLength(255);

            modelBuilder.Entity<IdentityRole>()
                .Property(r => r.Id)
                .HasMaxLength(255);
        }
    }
} 