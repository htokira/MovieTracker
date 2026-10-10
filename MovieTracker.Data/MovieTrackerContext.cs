using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Core.Models;

namespace MovieTracker.Data
{
    public class MovieTrackerContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public MovieTrackerContext(DbContextOptions<MovieTrackerContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(e =>
            {
                e.Property(u => u.DisplayName).IsRequired().HasMaxLength(50);
                e.Property(u => u.Bio).HasMaxLength(500);
                e.Property(u => u.AvatarUrl).HasMaxLength(500);
            });

            builder.Entity<RefreshToken>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
                e.HasIndex(t => t.TokenHash).IsUnique();
                e.HasOne(t => t.User)
                 .WithMany()
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
