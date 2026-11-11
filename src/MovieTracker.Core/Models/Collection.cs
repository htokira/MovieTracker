namespace MovieTracker.Core.Models
{
    public class Collection
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<CollectionMovie> Movies { get; set; } = new();
    }
}