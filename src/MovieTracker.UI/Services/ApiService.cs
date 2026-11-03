using System.Net.Http.Json;
using MovieTracker.Core.Dtos;

namespace MovieTracker.UI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:7226/"); 
    }

    public async Task<IEnumerable<MovieListItemDto>> GetPopularMoviesAsync(int page = 1)
    {
        var movies = await _httpClient.GetFromJsonAsync<IEnumerable<MovieListItemDto>>(
            $"api/movies/popular?page={page}");

        return movies ?? Enumerable.Empty<MovieListItemDto>();
    }

    public async Task<IEnumerable<MovieListItemDto>> GetMoviesAsync(string? query = null, int page = 1, 
    int? genreId = null, string? sortBy = null)
    {
        var parameters = new List<string>
        {
            $"page={page}"
        };

        if (!string.IsNullOrEmpty(query))
        {
            parameters.Add($"query={query}");
        }

        if (genreId.HasValue)
        {
            parameters.Add($"genreId={genreId.Value}");
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            parameters.Add($"sortBy={sortBy}");
        }

        var url = $"api/movies?{string.Join("&", parameters)}";
        var movies = await _httpClient.GetFromJsonAsync<IEnumerable<MovieListItemDto>>(url);
        return movies ?? Enumerable.Empty<MovieListItemDto>();
    }

    public async Task<MovieDetailDto?> GetMovieDetailsAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MovieDetailDto>($"api/movies/{id}");
        }
        catch
        {
            return null;
        }
    }
}