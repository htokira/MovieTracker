using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MovieTracker.Tests.Controllers;

public class TasteProfileControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TasteProfileControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest($"taste_user_{Guid.NewGuid():N}@example.com", "Password123!", "TasteTester");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetMyProfile_BeforeAnalysis_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/taste-profile/me", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Analyze_WithMultipleReviewsAndGenres_CalculatesMetricsCorrectly()
    {
        var client = await CreateAuthenticatedClientAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieTrackerContext>();

            var scifiGenre = new Genre { Id = 12, Name = "Science Fiction" };
            var horrorGenre = new Genre { Id = 27, Name = "Horror" };

            db.Genres.AddRange(scifiGenre, horrorGenre);

            db.Movies.AddRange(
                new Movie { Id = 101, Title = "Interstellar", MediaType = "Movie", Genres = new List<Genre> { scifiGenre } },
                new Movie { Id = 102, Title = "Arrival", MediaType = "Movie", Genres = new List<Genre> { scifiGenre } },
                new Movie { Id = 104, Title = "The Matrix", MediaType = "Movie", Genres = new List<Genre> { scifiGenre } },
                new Movie { Id = 105, Title = "Blade Runner", MediaType = "Movie", Genres = new List<Genre> { scifiGenre } },
                new Movie { Id = 103, Title = "Scary Movie", MediaType = "Movie", Genres = new List<Genre> { horrorGenre } },
                new Movie { Id = 106, Title = "Terrible Movie", MediaType = "Movie", Genres = new List<Genre> { horrorGenre } },
                new Movie { Id = 107, Title = "Terrifying Movie", MediaType = "Movie", Genres = new List<Genre> { horrorGenre } }
            );

            await db.SaveChangesAsync();
        }

        await client.PutAsJsonAsync("/api/movies/101/reviews/me", new UpsertReviewRequest(10, "Masterpiece"), TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/movies/102/reviews/me", new UpsertReviewRequest(10, "Amazing"), TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/movies/104/reviews/me", new UpsertReviewRequest(9, "Classic"), TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/movies/105/reviews/me", new UpsertReviewRequest(9, "Atmospheric"), TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/movies/103/reviews/me", new UpsertReviewRequest(2, "Terrible"), TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/movies/106/reviews/me", new UpsertReviewRequest(1, "Terrible"), TestContext.Current.CancellationToken);
        await client.PutAsJsonAsync("/api/movies/107/reviews/me", new UpsertReviewRequest(2, "Terrible"), TestContext.Current.CancellationToken);

        var analyzeResponse = await client.PostAsync("/api/taste-profile/analyze", null, TestContext.Current.CancellationToken);
        analyzeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await analyzeResponse.Content.ReadFromJsonAsync<TasteProfileDto>();

        profile.Should().NotBeNull();
        profile!.ReviewsCount.Should().Be(7);
        profile.FavoriteGenres.Should().Contain("Science Fiction");
        profile.FavoriteGenreIds.Should().NotBeEmpty();
        profile.DislikedGenres.Should().Contain("Horror");
        profile.DislikedGenreIds.Should().NotBeEmpty();
        profile.HighlyRatedMovieIds.Should().NotBeEmpty();
        profile.TasteDiversity.Should().BeGreaterThan(0);
        profile.Confidence.Should().BeGreaterThan(0);
    }
}