using System.Text.Json;
using Microsoft.Extensions.Options;
using MovieTracker.API.Settings;
using MovieTracker.Core.Dtos;

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

        public async Task<IEnumerable<MovieListItemDto>> GetPopularMoviesAsync(int page = 1)
        {
            var url = $"{_settings.BaseUrl}movie/popular?api_key={_settings.ApiKey}&language=uk-UA&page={page}";
            return await FetchMoviesAsync(url);
        }

        public async Task<IEnumerable<MovieListItemDto>> SearchMoviesAsync(string query, int page = 1)
        {
            var url = $"{_settings.BaseUrl}search/movie?api_key={_settings.ApiKey}&language=uk-UA&query={Uri.EscapeDataString(query)}&page={page}";
            return await FetchMoviesAsync(url);
        }

        private async Task<IEnumerable<MovieListItemDto>> FetchMoviesAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<MovieListItemDto>();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            
            var results = doc.RootElement.GetProperty("results");
            var movies = new List<MovieListItemDto>();

            foreach (var item in results.EnumerateArray())
            {
                movies.Add(new MovieListItemDto
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

        public async Task<MovieDetailDto?> GetMovieDetailsAsync(int id)
        {
            var url = $"{_settings.BaseUrl}movie/{id}?api_key={_settings.ApiKey}&language=uk-UA&append_to_response=credits";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var item = doc.RootElement;

            var movie = new MovieDetailDto
            {
                Id = item.TryGetProperty("id", out var movieId) ? movieId.GetInt32() : 0,
                Title = item.GetProperty("title").GetString() ?? string.Empty,
                Description = item.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
                ReleaseYear = item.TryGetProperty("release_date", out var releaseDate) 
                && DateTime.TryParse(releaseDate.GetString(), out var date) ? date.Year : null,
                Rating = item.GetProperty("vote_average").GetDouble(),
                MediaType = "Movie",
                PosterUrl = item.TryGetProperty("poster_path", out var poster) && poster.ValueKind != JsonValueKind.Null 
                    ? $"https://image.tmdb.org/t/p/w500{poster.GetString()}" 
                    : null
            };

            // Парсимо жанри
            if (item.TryGetProperty("genres", out var genresProp) && genresProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var genreEl in genresProp.EnumerateArray())
                {
                    movie.Genres.Add(new GenreDto
                    {
                        Id = genreEl.GetProperty("id").GetInt32(),
                        Name = genreEl.GetProperty("name").GetString() ?? string.Empty
                    });
                }
            }

            // Парсимо акторів (каст)
            if (item.TryGetProperty("credits", out var creditsProp) && 
                creditsProp.TryGetProperty("cast", out var castProp) && 
                castProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var castEl in castProp.EnumerateArray().Take(10))
                {
                    movie.MoviePersons.Add(new MoviePersonDto
                    {
                        FullName = castEl.GetProperty("name").GetString() ?? string.Empty,
                        PhotoUrl = castEl.TryGetProperty("profile_path", out var profile) && profile.ValueKind != JsonValueKind.Null
                            ? $"https://image.tmdb.org/t/p/w500{profile.GetString()}"
                            : null,
                        Role = castEl.TryGetProperty("character", out var charProp) ? charProp.GetString() ?? "Actor" : "Actor"
                    });
                }
            }

            return movie;
        }
    }
}