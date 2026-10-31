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

    public async Task<IEnumerable<MovieListItemDto>> GetMoviesAsync(string? query = null, int page = 1)
    {
        var url = string.IsNullOrWhiteSpace(query) 
            ? $"api/movies?page={page}" 
            : $"api/movies?query={Uri.EscapeDataString(query)}&page={page}";

        try
        {
            var movies = await _httpClient.GetFromJsonAsync<IEnumerable<MovieListItemDto>>(url);
            return movies ?? Enumerable.Empty<MovieListItemDto>();
        }
        catch
        {
            return Enumerable.Empty<MovieListItemDto>();
        }
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