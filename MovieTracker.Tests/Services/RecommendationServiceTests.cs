using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.Text.Json;

namespace MovieTracker.Tests.Services;

public class RecommendationServiceTests : IDisposable
{
    private const int TmdbPageSize = 20;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly MovieTrackerContext _db;
    private readonly Mock<ITmdbService> _tmdbMock = new();
    private readonly RecommendationService _service;

    public RecommendationServiceTests()
    {
        var options = new DbContextOptionsBuilder<MovieTrackerContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new MovieTrackerContext(options);
        _service = new RecommendationService(_db, _tmdbMock.Object);
    }

    public void Dispose() => _db.Dispose();

    private static MovieListItemDto Movie(int id, double rating = 7.0) => new()
    {
        Id = id,
        Title = $"Movie {id}",
        Rating = rating,
        MediaType = "Movie"
    };

    private static List<MovieListItemDto> Movies(int fromId, int count) =>
        Enumerable.Range(fromId, count).Select(id => Movie(id)).ToList();

    private void SetupPopular(List<MovieListItemDto> movies) =>
        _tmdbMock
            .Setup(s => s.GetPopularMoviesAsync(It.IsAny<int>()))
            .ReturnsAsync((int page) => movies.Skip((page - 1) * TmdbPageSize).Take(TmdbPageSize).ToList());

    private void SetupSimilar(int movieId, List<MovieListItemDto> movies) =>
        _tmdbMock
            .Setup(s => s.GetSimilarMoviesAsync(movieId, It.IsAny<int>()))
            .ReturnsAsync(movies);

    private void SetupDiscover(int genreId, List<MovieListItemDto> movies) =>
        _tmdbMock
            .Setup(s => s.GetDiscoverMoviesAsync(It.IsAny<int>(), genreId, It.IsAny<string?>()))
            .ReturnsAsync(movies);

    private async Task AddProfileAsync(List<int>? highlyRatedMovieIds = null, List<int>? favoriteGenreIds = null)
    {
        _db.TasteProfiles.Add(new TasteProfile
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            HighlyRatedMovieIdsJson = JsonSerializer.Serialize(highlyRatedMovieIds ?? new List<int>()),
            FavoriteGenreIdsJson = JsonSerializer.Serialize(favoriteGenreIds ?? new List<int>()),
            CalculatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private void AddReview(int movieId) =>
        _db.Reviews.Add(new Review { Id = Guid.NewGuid(), UserId = _userId, MovieId = movieId, Rating = 9 });

    [Fact]
    public async Task GetRecommendations_ExcludesReviewedAndTrackedMovies()
    {
        await AddProfileAsync(highlyRatedMovieIds: new() { 100 });
        SetupSimilar(100, new() { Movie(1), Movie(2), Movie(3), Movie(4) });
        SetupPopular(new() { Movie(2), Movie(3), Movie(5) });

        AddReview(100);
        AddReview(2);
        _db.UserMovieStatuses.Add(new UserMovieStatus
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            MovieId = 3,
            Status = WatchStatus.Watched
        });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _service.GetRecommendationsAsync(_userId);

        var ids = result.Items.Select(m => m.Id).ToList();
        result.IsPersonalized.Should().BeTrue();
        ids.Should().NotContain(new[] { 2, 3, 100 });
        ids.Should().Contain(new[] { 1, 4, 5 }); // 5 — дозаповнення популярними, теж без переглянутих
    }

    [Fact]
    public async Task GetRecommendations_ReturnsAtMost20UniqueMovies()
    {
        await AddProfileAsync(highlyRatedMovieIds: new() { 100, 101 }, favoriteGenreIds: new() { 28 });
        SetupSimilar(100, Movies(1, 20));
        SetupSimilar(101, Movies(15, 20));
        SetupDiscover(28, Movies(40, 20));
        SetupPopular(Movies(200, 40));

        var result = await _service.GetRecommendationsAsync(_userId);

        result.Items.Should().HaveCount(RecommendationService.MaxResults);
        result.Items.Select(m => m.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetRecommendations_WithoutProfile_FallsBackToPopular()
    {
        SetupPopular(Movies(1, 30));

        var result = await _service.GetRecommendationsAsync(_userId);

        result.IsPersonalized.Should().BeFalse();
        result.Items.Select(m => m.Id).Should().Equal(Enumerable.Range(1, 20));
        _tmdbMock.Verify(s => s.GetSimilarMoviesAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetRecommendations_WithEmptyProfile_FallsBackToPopular()
    {
        await AddProfileAsync();
        SetupPopular(Movies(1, 30));

        var result = await _service.GetRecommendationsAsync(_userId);

        result.IsPersonalized.Should().BeFalse();
        result.Items.Should().HaveCount(RecommendationService.MaxResults);
    }
}