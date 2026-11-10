using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace MovieTracker.Tests.Controllers
{
    public class MovieStatusesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public MovieStatusesControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // Створює нового користувача і клієнта з його JWT-токеном.
        // Кожен тест бере свого користувача, тому тести не заважають один одному.
        private static async Task<HttpClient> CreateAuthorizedClientAsync(WebApplicationFactory<Program> factory)
        {
            using var scope = factory.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var email = $"{Guid.NewGuid():N}@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                DisplayName = "Tester"
            };

            var result = await users.CreateAsync(user, "Password123");
            result.Succeeded.Should().BeTrue();

            var (token, _) = tokens.CreateAccessToken(user);

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static StringContent Json(string json) =>
            new(json, Encoding.UTF8, "application/json");

        // Фільм із одним жанром і одним актором (для тестів спільних Genres/Persons).
        // Якщо MovieDetailDto/GenreDto/MoviePersonDto відрізняються, підлаштуй типи під них.
        private static MovieDetailDto MakeMovie(int id, int genreId, int personId) => new()
        {
            Id = id,
            Title = $"Test Movie {id}",
            Description = "Test",
            ReleaseYear = 2020,
            Rating = 7.5,
            MediaType = "movie",
            PosterUrl = null,
            Genres = new() { new() { Id = genreId, Name = "Action" } },
            MoviePersons = new()
            {
                new() { PersonId = personId, FullName = "Test Actor", PhotoUrl = null, Role = "Actor" }
            }
        };

        // ---------- Без авторизації ----------

        [Fact]
        public async Task AllEndpoints_WithoutToken_ReturnUnauthorized()
        {
            var client = _factory.CreateClient();

            (await client.GetAsync("/api/movies/550/status")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.PutAsJsonAsync("/api/movies/550/status", new SetWatchStatusRequest(WatchStatus.Watched)))
                .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.DeleteAsync("/api/movies/550/status")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.GetAsync("/api/users/me/movies")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ---------- Створення і зміна ----------

        [Fact]
        public async Task SetStatus_NewMovie_CreatesStatus()
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            var response = await client.PutAsJsonAsync("/api/movies/550/status",
                new SetWatchStatusRequest(WatchStatus.Planned));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<WatchStatusResponse>();
            body!.MovieId.Should().Be(550);
            body.Status.Should().Be(WatchStatus.Planned);

            var get = await client.GetAsync("/api/movies/550/status");
            get.StatusCode.Should().Be(HttpStatusCode.OK);
            (await get.Content.ReadFromJsonAsync<WatchStatusResponse>())!.Status.Should().Be(WatchStatus.Planned);
        }

        [Fact]
        public async Task SetStatus_ExistingStatus_UpdatesInsteadOfDuplicating()
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            await client.PutAsJsonAsync("/api/movies/551/status", new SetWatchStatusRequest(WatchStatus.Planned));
            var response = await client.PutAsJsonAsync("/api/movies/551/status", new SetWatchStatusRequest(WatchStatus.Watched));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadFromJsonAsync<WatchStatusResponse>())!.Status.Should().Be(WatchStatus.Watched);

            var list = await client.GetFromJsonAsync<UserMoviesResponse>("/api/users/me/movies");
            list!.TotalCount.Should().Be(1);
            list.Items.Single().Status.Should().Be(WatchStatus.Watched);
        }

        [Fact]
        public async Task SetStatus_TwoMoviesSharingGenreAndPerson_ReusesThemWithoutDuplicates()
        {
            const int genreId = 9001;
            const int personId = 9002;

            var tmdb = new Mock<ITmdbService>();
            tmdb.Setup(s => s.GetMovieDetailsAsync(701)).ReturnsAsync(MakeMovie(701, genreId, personId));
            tmdb.Setup(s => s.GetMovieDetailsAsync(702)).ReturnsAsync(MakeMovie(702, genreId, personId));

            var factory = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ITmdbService>();
                    services.AddSingleton(tmdb.Object);
                }));
            var client = await CreateAuthorizedClientAsync(factory);

            var first = await client.PutAsJsonAsync("/api/movies/701/status", new SetWatchStatusRequest(WatchStatus.Planned));
            var second = await client.PutAsJsonAsync("/api/movies/702/status", new SetWatchStatusRequest(WatchStatus.Planned));
            // Повторний запит на вже збережений фільм: TMDB вдруге не викликається
            var repeat = await client.PutAsJsonAsync("/api/movies/701/status", new SetWatchStatusRequest(WatchStatus.Watched));

            first.StatusCode.Should().Be(HttpStatusCode.OK);
            second.StatusCode.Should().Be(HttpStatusCode.OK);
            repeat.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MovieTrackerContext>();

            (await db.Genres.CountAsync(g => g.Id == genreId)).Should().Be(1);
            (await db.Persons.CountAsync(p => p.Id == personId)).Should().Be(1);
            (await db.Movies.CountAsync(m => m.Id == 701 || m.Id == 702)).Should().Be(2);

            tmdb.Verify(s => s.GetMovieDetailsAsync(701), Times.Once);
            tmdb.Verify(s => s.GetMovieDetailsAsync(702), Times.Once);
        }

        // ---------- Валідація ----------

        [Theory]
        [InlineData("{\"status\": 99}")]
        [InlineData("{\"status\": 0}")]
        [InlineData("{\"status\": \"Unknown\"}")]
        [InlineData("{}")]
        public async Task SetStatus_InvalidStatus_ReturnsBadRequest(string json)
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            var response = await client.PutAsync("/api/movies/552/status", Json(json));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await client.GetAsync("/api/movies/552/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SetStatus_InvalidMovieId_ReturnsNotFound()
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            var response = await client.PutAsJsonAsync("/api/movies/0/status",
                new SetWatchStatusRequest(WatchStatus.Watched));

            // Маршрут {movieId:int:min(1)} не збігається з 0
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SetStatus_MovieNotFoundInTmdb_ReturnsNotFound()
        {
            var factory = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ITmdbService>();
                    var mock = new Mock<ITmdbService>();
                    mock.Setup(s => s.GetMovieDetailsAsync(It.IsAny<int>()))
                        .ReturnsAsync((MovieDetailDto?)null);
                    services.AddSingleton(mock.Object);
                }));
            var client = await CreateAuthorizedClientAsync(factory);

            var response = await client.PutAsJsonAsync("/api/movies/999999/status",
                new SetWatchStatusRequest(WatchStatus.Watched));

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ---------- Читання і видалення ----------

        [Fact]
        public async Task GetStatus_NoStatus_ReturnsNotFound()
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            (await client.GetAsync("/api/movies/553/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteStatus_ExistingStatus_RemovesIt()
        {
            var client = await CreateAuthorizedClientAsync(_factory);
            await client.PutAsJsonAsync("/api/movies/554/status", new SetWatchStatusRequest(WatchStatus.Dropped));

            var delete = await client.DeleteAsync("/api/movies/554/status");

            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await client.GetAsync("/api/movies/554/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteStatus_NoStatus_ReturnsNotFound()
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            (await client.DeleteAsync("/api/movies/555/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ---------- Чужі записи ----------

        [Fact]
        public async Task Status_IsVisibleAndDeletableOnlyByItsOwner()
        {
            var owner = await CreateAuthorizedClientAsync(_factory);
            var stranger = await CreateAuthorizedClientAsync(_factory);
            await owner.PutAsJsonAsync("/api/movies/556/status", new SetWatchStatusRequest(WatchStatus.Watched));

            (await stranger.GetAsync("/api/movies/556/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await stranger.DeleteAsync("/api/movies/556/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await owner.GetAsync("/api/movies/556/status")).StatusCode.Should().Be(HttpStatusCode.OK);

            var strangerList = await stranger.GetFromJsonAsync<UserMoviesResponse>("/api/users/me/movies");
            strangerList!.TotalCount.Should().Be(0);
        }

        // ---------- Список ----------

        [Fact]
        public async Task GetMyMovies_FiltersByStatus()
        {
            var client = await CreateAuthorizedClientAsync(_factory);
            await client.PutAsJsonAsync("/api/movies/601/status", new SetWatchStatusRequest(WatchStatus.Watched));
            await client.PutAsJsonAsync("/api/movies/602/status", new SetWatchStatusRequest(WatchStatus.Planned));
            await client.PutAsJsonAsync("/api/movies/603/status", new SetWatchStatusRequest(WatchStatus.Watched));

            var all = await client.GetFromJsonAsync<UserMoviesResponse>("/api/users/me/movies");
            var watched = await client.GetFromJsonAsync<UserMoviesResponse>("/api/users/me/movies?status=Watched");

            all!.TotalCount.Should().Be(3);
            watched!.TotalCount.Should().Be(2);
            watched.Items.Should().OnlyContain(i => i.Status == WatchStatus.Watched);
            watched.Items.Select(i => i.MovieId).Should().BeEquivalentTo(new[] { 601, 603 });
            watched.Items.First().Title.Should().StartWith("Test Movie");
        }

        [Theory]
        [InlineData("status=abc")]
        [InlineData("status=99")]
        [InlineData("page=0")]
        [InlineData("pageSize=100")]
        public async Task GetMyMovies_InvalidQuery_ReturnsBadRequest(string query)
        {
            var client = await CreateAuthorizedClientAsync(_factory);

            (await client.GetAsync($"/api/users/me/movies?{query}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}