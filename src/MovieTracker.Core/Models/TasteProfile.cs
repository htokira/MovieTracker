namespace MovieTracker.Core.Models
{
    public class TasteProfile
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int ReviewsCount { get; set; }

        public string GenrePreferencesJson { get; set; } = "[]";

        public string FavoriteGenresJson { get; set; } = "[]";
        public string DislikedGenresJson { get; set; } = "[]";

        public double TasteDiversity { get; set; }

        public double TasteConsistency { get; set; }

        public double Confidence { get; set; }

        public DateTime CalculatedAt { get; set; }
    }
}