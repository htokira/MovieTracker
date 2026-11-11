namespace MovieTracker.Core.Models
{
    public class CollectionMovie
    {
        public Guid CollectionId { get; set; }
        public Collection Collection { get; set; } = null!;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}