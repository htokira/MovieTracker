using MovieTracker.Core.Models;

namespace MovieTracker.API.Services
{
    public interface ITmdbService
    {
        Task<IEnumerable<Movie>> GetPopularMoviesAsync(int page = 1);
        Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int page = 1);
    }
}