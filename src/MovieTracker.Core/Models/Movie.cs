namespace MovieTracker.Core.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public double Rating { get; set; }
        public string MediaType { get; set; } = "Movie"; // Movie або Series
        public string? PosterUrl { get; set; }

        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public ICollection<MoviePerson> MoviePersons { get; set; } = new List<MoviePerson>();
    }
}