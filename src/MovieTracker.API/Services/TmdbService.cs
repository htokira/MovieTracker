using System.Text.Json;
using Microsoft.Extensions.Options;
using MovieTracker.API.Settings;
using MovieTracker.Core.Models;

namespace MovieTracker.API.Services
{
    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly TmdbSettings _settings;

        public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<IEnumerable<Movie>> GetPopularMoviesAsync(int page = 1)
        {
            var url = $"{_settings.BaseUrl}movie/popular?api_key={_settings.ApiKey}&language=uk-UA&page={page}";
            return await FetchMoviesAsync(url);
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int page = 1)
        {
            var url = $"{_settings.BaseUrl}search/movie?api_key={_settings.ApiKey}&language=uk-UA&query={Uri.EscapeDataString(query)}&page={page}";
            return await FetchMoviesAsync(url);
        }

        private async Task<IEnumerable<Movie>> FetchMoviesAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Movie>();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            
            var results = doc.RootElement.GetProperty("results");
            var movies = new List<Movie>();

            foreach (var item in results.EnumerateArray())
            {
                movies.Add(new Movie
                {
                    Id = item.GetProperty("id").GetInt32(),
                    Title = item.GetProperty("title").GetString() ?? string.Empty,
                    Description = item.GetProperty("overview").GetString() ?? string.Empty,
                    ReleaseYear = DateTime.TryParse(item.GetProperty("release_date").GetString(), out var date) ? date.Year : 0,
                    Rating = item.GetProperty("vote_average").GetDouble(),
                    MediaType = "Movie",
                    PosterUrl = item.TryGetProperty("poster_path", out var poster) && poster.ValueKind != JsonValueKind.Null 
                        ? $"https://image.tmdb.org/t/p/w500{poster.GetString()}" 
                        : null
                });
            }

            return movies;
        }
    }
}