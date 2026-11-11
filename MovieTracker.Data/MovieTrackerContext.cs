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
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<MoviePerson> MoviePersons => Set<MoviePerson>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<UserMovieStatus> UserMovieStatuses => Set<UserMovieStatus>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<CollectionMovie> CollectionMovies => Set<CollectionMovie>();

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

            builder.Entity<Review>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.Text).HasMaxLength(2000);

                e.HasIndex(r => new { r.UserId, r.MovieId }).IsUnique();
                e.HasIndex(r => r.MovieId);

                e.HasOne(r => r.Movie)
                 .WithMany(m => m.Reviews)
                 .HasForeignKey(r => r.MovieId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(r => r.User)
                 .WithMany()
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "Rating >= 1 AND Rating <= 10"));
            });

            builder.Entity<UserMovieStatus>(e =>
            {
                e.HasKey(s => s.Id);

                e.HasIndex(s => new { s.UserId, s.MovieId }).IsUnique();
                e.HasIndex(s => new { s.UserId, s.Status });

                e.HasOne(s => s.Movie)
                 .WithMany()
                 .HasForeignKey(s => s.MovieId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(s => s.User)
                 .WithMany()
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Collection>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Name).IsRequired().HasMaxLength(100);
                e.Property(c => c.Description).HasMaxLength(1000);

                e.HasIndex(c => c.UserId);
                e.HasIndex(c => c.IsPublic);

                e.HasOne(c => c.User)
                 .WithMany()
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CollectionMovie>(e =>
            {
                e.HasKey(cm => new { cm.CollectionId, cm.MovieId });

                e.HasOne(cm => cm.Collection)
                 .WithMany(c => c.Movies)
                 .HasForeignKey(cm => cm.CollectionId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(cm => cm.Movie)
                 .WithMany()
                 .HasForeignKey(cm => cm.MovieId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Movie>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Id).ValueGeneratedNever();
                e.Property(m => m.Title).IsRequired().HasMaxLength(255);
                e.Property(m => m.Description).HasMaxLength(4000);
                e.Property(m => m.PosterUrl).HasMaxLength(500);
                e.Property(m => m.MediaType).IsRequired().HasMaxLength(50);

                e.HasMany(m => m.Genres)
                 .WithMany(g => g.Movies);

                e.HasMany(m => m.MoviePersons)
                 .WithOne(mp => mp.Movie)
                 .HasForeignKey(mp => mp.MovieId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Person>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Id).ValueGeneratedNever();
                e.Property(p => p.FullName)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(p => p.PhotoUrl)
                 .HasMaxLength(500);
            });

            builder.Entity<MoviePerson>(e =>
            {
                e.HasKey(mp => new { mp.MovieId, mp.PersonId });

                e.HasOne(mp => mp.Movie)
                 .WithMany(m => m.MoviePersons)
                 .HasForeignKey(mp => mp.MovieId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(mp => mp.Person)
                 .WithMany(p => p.MoviePersons)
                 .HasForeignKey(mp => mp.PersonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Genre>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.Id).ValueGeneratedNever();
                e.Property(g => g.Name).IsRequired().HasMaxLength(100);
            });
        }
    }
}