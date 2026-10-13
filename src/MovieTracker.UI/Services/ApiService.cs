using System.Net.Http.Json;
using MovieTracker.Core.Models;

namespace MovieTracker.UI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7226/"); 
    }

    public async Task<IEnumerable<Movie>> GetMoviesAsync(string? query = null, int page = 1)
    {
        var url = string.IsNullOrWhiteSpace(query) 
            ? $"api/movies?page={page}" 
            : $"api/movies?query={Uri.EscapeDataString(query)}&page={page}";

        try
        {
            var movies = await _httpClient.GetFromJsonAsync<IEnumerable<Movie>>(url);
            return movies ?? Enumerable.Empty<Movie>();
        }
        catch
        {
            return Enumerable.Empty<Movie>();
        }
    }
}