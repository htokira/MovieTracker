namespace MovieTracker.Core.Dtos
{
    public class PagedResultDto<T>
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}