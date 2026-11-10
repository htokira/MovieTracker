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

        private static readonly Dictionary<int, string> GenreDictionary = new()
        {
            { 28, "Бойовик" }, { 12, "Пригоди" }, { 16, "Анімація" }, { 35, "Комедія" },
            { 80, "Кримінал" }, { 99, "Документальний" }, { 18, "Драма" }, { 10751, "Сімейний" },
            { 14, "Фентезі" }, { 36, "Історія" }, { 27, "Жахи" }, { 10402, "Музика" },
            { 9648, "Детектив" }, { 10749, "Мелодрама" }, { 878, "Наукова фантастика" },
            { 10770, "Телефільм" }, { 53, "Трилер" }, { 10752, "Військовий" }, { 37, "Вестерн" }
        };

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

        public async Task<IEnumerable<MovieListItemDto>> GetDiscoverMoviesAsync(int page = 1, int? genreId = null, string? sortBy = null)
        {
            var sort = string.IsNullOrEmpty(sortBy) ? "popularity.desc" : sortBy;

            var url = $"{_settings.BaseUrl}discover/movie?api_key={_settings.ApiKey}&language=uk-UA&sort_by={sort}&page={page}";
            
            if (genreId.HasValue)
            {
                url += $"&with_genres={genreId.Value}";
            }
            
            return await FetchMoviesAsync(url);
        }

        public async Task<IEnumerable<MovieListItemDto>> SearchMoviesAsync(string query, int page = 1, int? genreId = null)
        {
            var url = $"{_settings.BaseUrl}search/movie?api_key={_settings.ApiKey}&language=uk-UA&query={Uri.EscapeDataString(query)}&page={page}";
            var movies = await FetchMoviesAsync(url);

            return genreId.HasValue 
                ? movies.Where(m => m.Genres.Any(g => g.Id == genreId.Value)) 
                : movies;
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
                var movieDto = new MovieListItemDto
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
                };

                // Парсимо жанри
                if (item.TryGetProperty("genre_ids", out var genreIdsProp) && genreIdsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var genreIdEl in genreIdsProp.EnumerateArray())
                    {
                        var genreId = genreIdEl.GetInt32();
                        if (GenreDictionary.TryGetValue(genreId, out var genreName))
                        {
                            movieDto.Genres.Add(new GenreDto { Id = genreId, Name = genreName });
                        }
                    }
                }

                movies.Add(movieDto);
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
                        PersonId = castEl.GetProperty("id").GetInt32(),
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