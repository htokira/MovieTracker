using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MovieTracker.API.Services;
using MovieTracker.API.Settings;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace MovieTracker.Tests.Services
{
    public class TmdbServiceTests
    {
        private readonly TmdbSettings _settings;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly TmdbService _tmdbService;

        public TmdbServiceTests()
        {
            _settings = new TmdbSettings
            {
                BaseUrl = "https://api.themoviedb.org/3/",
                ApiKey = "test_api_key"
            };

            var optionsMock = Options.Create(_settings);

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(_settings.BaseUrl)
            };

            _tmdbService = new TmdbService(_httpClient, optionsMock);
        }

        private void SetupHttpMessageHandlerResponse(HttpStatusCode statusCode, string jsonContent)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(jsonContent)
                });
        }

        [Fact]
        public async Task GetPopularMoviesAsync_WhenApiReturnsSuccess_ReturnsPagedResult()
        {
            var jsonResponse = JsonSerializer.Serialize(new
            {
                page = 1,
                total_pages = 5,
                total_results = 100,
                results = new[]
                {
                    new
                    {
                        id = 101,
                        title = "Test Movie",
                        overview = "Test Overview",
                        release_date = "2023-05-15",
                        vote_average = 7.5,
                        poster_path = "/test.jpg",
                        genre_ids = new[] { 28, 12 }
                    }
                }
            });

            SetupHttpMessageHandlerResponse(HttpStatusCode.OK, jsonResponse);

            var result = await _tmdbService.GetPopularMoviesAsync(1);

            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.TotalPages.Should().Be(5);
            result.TotalResults.Should().Be(100);
            result.Items.Should().HaveCount(1);

            var movie = result.Items.First();
            movie.Id.Should().Be(101);
            movie.Title.Should().Be("Test Movie");
            movie.Description.Should().Be("Test Overview");
            movie.ReleaseYear.Should().Be(2023);
            movie.Rating.Should().Be(7.5);
            movie.PosterUrl.Should().Be("https://image.tmdb.org/t/p/w500/test.jpg");
            movie.Genres.Should().HaveCount(2);
            movie.Genres.Select(g => g.Name).Should().ContainInOrder("Бойовик", "Пригоди");
        }

        [Fact]
        public async Task GetPopularMoviesAsync_WhenApiFails_ReturnsEmptyPagedResult()
        {
            SetupHttpMessageHandlerResponse(HttpStatusCode.InternalServerError, string.Empty);

            var result = await _tmdbService.GetPopularMoviesAsync(1);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task SearchMoviesAsync_WithGenreFilter_FiltersItemsCorrectly()
        {
            var jsonResponse = JsonSerializer.Serialize(new
            {
                page = 1,
                total_pages = 1,
                total_results = 2,
                results = new[]
                {
                    new
                    {
                        id = 1,
                        title = "Action Movie",
                        overview = "Action desc",
                        release_date = "2022-01-01",
                        vote_average = 8.0,
                        poster_path = (string?)null,
                        genre_ids = new[] { 28 }
                    },
                    new
                    {
                        id = 2,
                        title = "Comedy Movie",
                        overview = "Comedy desc",
                        release_date = "2022-01-01",
                        vote_average = 6.0,
                        poster_path = (string?)null,
                        genre_ids = new[] { 35 }
                    }
                }
            });

            SetupHttpMessageHandlerResponse(HttpStatusCode.OK, jsonResponse);

            var result = await _tmdbService.SearchMoviesAsync("Movie", 1, genreId: 28);

            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Action Movie");
            result.Items.First().Genres.Should().ContainSingle(g => g.Id == 28);
        }

        [Fact]
        public async Task GetMovieDetailsAsync_WhenMovieExists_ReturnsMovieDetailDto()
        {
            var jsonResponse = JsonSerializer.Serialize(new
            {
                id = 555,
                title = "Detailed Movie",
                overview = "Detailed overview text",
                release_date = "2021-10-10",
                vote_average = 8.9,
                poster_path = "/poster555.jpg",
                genres = new[]
                {
                    new { id = 18, name = "Драма" }
                },
                credits = new
                {
                    cast = new[]
                    {
                        new
                        {
                            id = 99,
                            name = "John Doe",
                            profile_path = "/actor99.jpg",
                            character = "Hero"
                        }
                    }
                }
            });

            SetupHttpMessageHandlerResponse(HttpStatusCode.OK, jsonResponse);

            var result = await _tmdbService.GetMovieDetailsAsync(555);

            result.Should().NotBeNull();
            result.Id.Should().Be(555);
            result.Title.Should().Be("Detailed Movie");
            result.Description.Should().Be("Detailed overview text");
            result.ReleaseYear.Should().Be(2021);
            result.Rating.Should().Be(8.9);
            result.PosterUrl.Should().Be("https://image.tmdb.org/t/p/w500/poster555.jpg");

            result.Genres.Should().ContainSingle(g => g.Id == 18 && g.Name == "Драма");

            result.MoviePersons.Should().ContainSingle();
            var person = result.MoviePersons[0];
            person.PersonId.Should().Be(99);
            person.FullName.Should().Be("John Doe");
            person.PhotoUrl.Should().Be("https://image.tmdb.org/t/p/w500/actor99.jpg");
            person.Role.Should().Be("Hero");
        }

        [Fact]
        public async Task GetMovieDetailsAsync_WhenNotFound_ReturnsNull()
        {
            SetupHttpMessageHandlerResponse(HttpStatusCode.NotFound, string.Empty);
            var result = await _tmdbService.GetMovieDetailsAsync(99999);
            result.Should().BeNull();
        }
    }
}