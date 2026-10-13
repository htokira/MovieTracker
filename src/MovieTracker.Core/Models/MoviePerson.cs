namespace MovieTracker.Core.Models
{
    public class MoviePerson
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;

        public string Role { get; set; } = "Actor";
    }
}