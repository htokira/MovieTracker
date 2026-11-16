using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MovieTracker.Core.Dtos;
using MovieTracker.Data;
using MovieTracker.Core.Models;
using MovieTracker.API.Services;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace MovieTracker.Tests.Controllers
{
    public class MoviesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public MoviesControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static string MoviesUrl(string query = "") => $"/api/movies{query}";
        private static string MovieByIdUrl(int id) => $"/api/movies/{id}";
        private static string PopularMoviesUrl(int page = 1) => $"/api/movies/popular?page={page}";

        [Fact]
        public async Task GetMovies_WithoutQuery_CallsDiscoverAndReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(MoviesUrl(), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<object>(cancellationToken: TestContext.Current.CancellationToken);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetMovies_WithQuery_CallsSearchAndReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(MoviesUrl("?query=Matrix&page=1"), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<object>(cancellationToken: TestContext.Current.CancellationToken);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPopularMovies_ReturnsOkWithData()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync(PopularMoviesUrl(1), TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMovieById_WhenExistsLocally_ReturnsLocalMovieDto()
        {
            var client = _factory.CreateClient();
            int localMovieId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MovieTrackerContext>();
                var movie = new Movie
                {
                    Title = "Inception",
                    Description = "A thief who steals corporate secrets...",
                    ReleaseYear = 2010,
                    Rating = 8.8,
                    MediaType = "Movie",
                    PosterUrl = "https://example.com/inception.jpg"
                };
                db.Movies.Add(movie);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
                localMovieId = movie.Id;
            }

            var response = await client.GetAsync(MovieByIdUrl(localMovieId), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var movieDto = await response.Content.ReadFromJsonAsync<MovieDetailDto>(cancellationToken: TestContext.Current.CancellationToken);
            
            movieDto.Should().NotBeNull();
            movieDto.Id.Should().Be(localMovieId);
            movieDto.Title.Should().Be("Inception");
            movieDto.ReleaseYear.Should().Be(2010);
        }

        [Fact]
        public async Task GetMovieById_WhenNotLocal_FetchesFromTmdbServiceAndReturnsOk()
        {
            var client = _factory.CreateClient();
            var nonLocalId = 99999;

            var response = await client.GetAsync(MovieByIdUrl(nonLocalId), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var movieDto = await response.Content.ReadFromJsonAsync<MovieDetailDto>(cancellationToken: TestContext.Current.CancellationToken);
            
            movieDto.Should().NotBeNull();
            movieDto.Id.Should().Be(nonLocalId);
            movieDto.Title.Should().Be($"Test Movie {nonLocalId}");
        }

        [Fact]
        public async Task GetMovieById_WhenNotFoundAnywhere_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var missingId = 404404;
            var response = await client.GetAsync(MovieByIdUrl(missingId), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}