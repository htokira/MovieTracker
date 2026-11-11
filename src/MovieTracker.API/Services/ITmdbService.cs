using MovieTracker.Core.Models;
using MovieTracker.Core.Dtos;

namespace MovieTracker.API.Services
{
    public interface ITmdbService
    {
        Task<IEnumerable<MovieListItemDto>> GetPopularMoviesAsync(int page = 1);
        Task<IEnumerable<MovieListItemDto>> GetDiscoverMoviesAsync(int page = 1, int? genreId = null, string? sortBy = null);
        Task<IEnumerable<MovieListItemDto>> SearchMoviesAsync(string query, int page = 1, int? genreId = null);
        Task<IEnumerable<MovieListItemDto>> GetSimilarMoviesAsync(int movieId, int page = 1);
        Task<MovieDetailDto?> GetMovieDetailsAsync(int id);
    }
}