using MovieTracker.Core.Models;
using MovieTracker.Core.Dtos;

namespace MovieTracker.API.Services
{
    public interface ITmdbService
    {
        Task<PagedResultDto<MovieListItemDto>> GetPopularMoviesAsync(int page = 1);
        Task<PagedResultDto<MovieListItemDto>> GetDiscoverMoviesAsync(int page = 1, int? genreId = null, string? sortBy = null);
        Task<PagedResultDto<MovieListItemDto>> SearchMoviesAsync(string query, int page = 1, int? genreId = null);
        Task<PagedResultDto<MovieListItemDto>> GetSimilarMoviesAsync(int movieId, int page = 1);
        Task<MovieDetailDto?> GetMovieDetailsAsync(int id);
    }
}