using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MovieTracker.Tests.Controllers
{
    public class PublicUserDataSeededTests : IClassFixture<TestWebApplicationFactory>
    {
        private static int _lastMovieId = 900_000;

        private readonly TestWebApplicationFactory _factory;

        public PublicUserDataSeededTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static int NewMovieId() => Interlocked.Increment(ref _lastMovieId);

        private async Task<(HttpClient Client, Guid UserId)> CreateUserAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var email = $"{Guid.NewGuid():N}@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                DisplayName = "Tester" + Guid.NewGuid().ToString("N")[..6]
            };

            var result = await users.CreateAsync(user, "Password123");
            result.Succeeded.Should().BeTrue();

            var (token, _) = tokens.CreateAccessToken(user);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return (client, user.Id);
        }

        private async Task<int> SeedMovieAsync(string title, int? year = 2020)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MovieTrackerContext>();

            var id = NewMovieId();
            db.Movies.Add(new Movie { Id = id, Title = title, ReleaseYear = year });
            await db.SaveChangesAsync();
            return id;
        }

        private async Task SeedReviewAsync(
            Guid userId, int movieId, int rating, string? text, DateTime createdAt)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MovieTrackerContext>();

            db.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MovieId = movieId,
                Rating = rating,
                Text = text,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });
            await db.SaveChangesAsync();
        }

        private async Task SeedTasteProfileAsync(Guid userId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MovieTrackerContext>();

            db.Set<TasteProfile>().Add(new TasteProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReviewsCount = 5,
                FavoriteGenreIdsJson = "[28,12]",
                Confidence = 0.7,
                CalculatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task GetUserReviews_ReturnsOnlyThatUsersReviews_NewestFirst()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var (_, otherId) = await CreateUserAsync();
            var oldMovie = await SeedMovieAsync("Old one");
            var newMovie = await SeedMovieAsync("New one");
            var otherMovie = await SeedMovieAsync("Other user's");

            await SeedReviewAsync(userId, oldMovie, 6, null, DateTime.UtcNow.AddHours(-2));
            await SeedReviewAsync(userId, newMovie, 9, "great", DateTime.UtcNow.AddHours(-1));
            await SeedReviewAsync(otherId, otherMovie, 3, null, DateTime.UtcNow);

            var result = await _factory.CreateClient().GetFromJsonAsync<UserReviewsResponse>(
                $"/api/users/{userId}/reviews", ct);

            result!.TotalCount.Should().Be(2);
            result.Items.Select(i => i.MovieId).Should().Equal(newMovie, oldMovie);
        }

        [Fact]
        public async Task GetUserReviews_IncludesMovieInfo()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var movieId = await SeedMovieAsync("Some Movie", 1999);
            await SeedReviewAsync(userId, movieId, 8, "nice", DateTime.UtcNow);

            var result = await _factory.CreateClient().GetFromJsonAsync<UserReviewsResponse>(
                $"/api/users/{userId}/reviews", ct);

            var item = result!.Items.Should().ContainSingle().Subject;
            item.MovieTitle.Should().Be("Some Movie");
            item.ReleaseYear.Should().Be(1999);
            item.Rating.Should().Be(8);
            item.Text.Should().Be("nice");
        }

        [Fact]
        public async Task GetUserReviews_Pagination_ReturnsRequestedPage()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            for (var i = 0; i < 3; i++)
            {
                var movieId = await SeedMovieAsync($"Movie {i}");
                await SeedReviewAsync(userId, movieId, 5, null, DateTime.UtcNow.AddMinutes(-i));
            }

            var result = await _factory.CreateClient().GetFromJsonAsync<UserReviewsResponse>(
                $"/api/users/{userId}/reviews?page=2&pageSize=2", ct);

            result!.TotalCount.Should().Be(3);
            result.Page.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserTasteProfile_Anonymous_ReturnsProfile()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            await SeedTasteProfileAsync(userId);

            var profile = await _factory.CreateClient().GetFromJsonAsync<TasteProfileDto>(
                $"/api/users/{userId}/taste-profile", ct);

            profile!.ReviewsCount.Should().Be(5);
            profile.FavoriteGenreIds.Should().Equal(28, 12);
            profile.Confidence.Should().Be(0.7);
        }

        [Fact]
        public async Task GetUserTasteProfile_AliasRoute_ReturnsSameProfile()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            await SeedTasteProfileAsync(userId);

            var profile = await _factory.CreateClient().GetFromJsonAsync<TasteProfileDto>(
                $"/api/taste-profile/{userId}", ct);

            profile!.ReviewsCount.Should().Be(5);
        }

        [Fact]
        public async Task GetMyTasteProfile_ReturnsOwnProfile()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, userId) = await CreateUserAsync();
            await SeedTasteProfileAsync(userId);

            var profile = await client.GetFromJsonAsync<TasteProfileDto>("/api/taste-profile/me", ct);

            profile!.FavoriteGenreIds.Should().Equal(28, 12);
        }
    }
}
