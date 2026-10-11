namespace MovieTracker.Core.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        public ICollection<MoviePerson> MoviePersons { get; set; } = new List<MoviePerson>();
    }
}