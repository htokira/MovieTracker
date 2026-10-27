namespace MovieTracker.Core.Dtos
{
    public class MovieListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public double Rating { get; set; }
        public string MediaType { get; set; } = "Movie";
        public string? PosterUrl { get; set; }
    }

    public class MovieDetailDto : MovieListItemDto
    {
        public List<GenreDto> Genres { get; set; } = new();
        public List<MoviePersonDto> MoviePersons { get; set; } = new();
    }

    public class GenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class MoviePersonDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
