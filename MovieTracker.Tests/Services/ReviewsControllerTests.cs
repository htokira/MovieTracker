using FluentAssertions;
using MovieTracker.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace MovieTracker.Tests.Services
{
    public class ReviewsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private static int _lastMovieId = 1000;
        private readonly TestWebApplicationFactory _factory;

        public ReviewsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static int NewMovieId() => Interlocked.Increment(ref _lastMovieId);

        private static string MyReviewUrl(int movieId) => $"/api/movies/{movieId}/reviews/me";

        private async Task<HttpClient> CreateAuthenticatedClientAsync(string displayName = "Reviewer")
        {
            var client = _factory.CreateClient();
            var request = new RegisterRequest($"user_{Guid.NewGuid():N}@example.com", "StrongPassword123!", displayName);

            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            response.EnsureSuccessStatusCode();

            var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
            return client;
        }

        private static async Task<MovieReviewsResponse> GetReviewsAsync(HttpClient client, int movieId, string query = "")
        {
            var response = await client.GetAsync($"/api/movies/{movieId}/reviews{query}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            return (await response.Content.ReadFromJsonAsync<MovieReviewsResponse>())!;
        }

        [Fact]
        public async Task Upsert_NewReview_ReturnsReviewWithAuthorAndTrimmedText()
        {
            var movieId = NewMovieId();
            var client = await CreateAuthenticatedClientAsync("Alice");

            var response = await client.PutAsJsonAsync(MyReviewUrl(movieId), new UpsertReviewRequest(8, "  Great film  "), cancellationToken: TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var review = await response.Content.ReadFromJsonAsync<ReviewResponse>(cancellationToken: TestContext.Current.CancellationToken);
            review!.MovieId.Should().Be(movieId);
            review.Rating.Should().Be(8);
            review.Text.Should().Be("Great film");
            review.AuthorName.Should().Be("Alice");
        }

        [Fact]
        public async Task Upsert_SecondTime_UpdatesExistingReviewInsteadOfDuplicating()
        {
            var movieId = NewMovieId();
            var client = await CreateAuthenticatedClientAsync();
            await client.PutAsJsonAsync(MyReviewUrl(movieId), new UpsertReviewRequest(5, "Meh"), cancellationToken: TestContext.Current.CancellationToken);

            var response = await client.PutAsJsonAsync(MyReviewUrl(movieId), new UpsertReviewRequest(9, "Changed my mind"), cancellationToken: TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var reviews = await GetReviewsAsync(client, movieId);
            reviews.TotalReviews.Should().Be(1);
            reviews.Items.Single().Rating.Should().Be(9);
            reviews.Items.Single().Text.Should().Be("Changed my mind");
        }

        [Fact]
        public async Task Upsert_RatingWithoutText_IsAllowed()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.PutAsJsonAsync(MyReviewUrl(NewMovieId()), new UpsertReviewRequest(7, "   "), cancellationToken: TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var review = await response.Content.ReadFromJsonAsync<ReviewResponse>(cancellationToken: TestContext.Current.CancellationToken);
            review!.Text.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(-5)]
        public async Task Upsert_RatingOutOfRange_ReturnsBadRequest(int rating)
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.PutAsJsonAsync(MyReviewUrl(NewMovieId()), new UpsertReviewRequest(rating, "text"), cancellationToken: TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Upsert_TextTooLong_ReturnsBadRequest()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.PutAsJsonAsync(MyReviewUrl(NewMovieId()), new UpsertReviewRequest(5, new string('a', 2001)), cancellationToken: TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Upsert_WithoutToken_ReturnsUnauthorized()
        {
            var anonymous = _factory.CreateClient();

            var response = await anonymous.PutAsJsonAsync(MyReviewUrl(NewMovieId()), new UpsertReviewRequest(5, null), cancellationToken: TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetReviews_ReturnsCountAndRoundedAverage()
        {
            var movieId = NewMovieId();
            foreach (var rating in new[] { 7, 8, 8 })
            {
                var user = await CreateAuthenticatedClientAsync();
                await user.PutAsJsonAsync(MyReviewUrl(movieId), new UpsertReviewRequest(rating, null), cancellationToken: TestContext.Current.CancellationToken);
            }

            var reviews = await GetReviewsAsync(_factory.CreateClient(), movieId);

            reviews.TotalReviews.Should().Be(3);
            reviews.AverageRating.Should().Be(7.7);
            reviews.Items.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetReviews_ForMovieWithoutReviews_ReturnsEmptyListAndNullAverage()
        {
            var reviews = await GetReviewsAsync(_factory.CreateClient(), NewMovieId());

            reviews.TotalReviews.Should().Be(0);
            reviews.AverageRating.Should().BeNull();
            reviews.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReviews_IsPaginatedNewestFirst()
        {
            var movieId = NewMovieId();
            foreach (var rating in new[] { 1, 2, 3 })
            {
                var user = await CreateAuthenticatedClientAsync();
                await user.PutAsJsonAsync(MyReviewUrl(movieId), new UpsertReviewRequest(rating, null), cancellationToken: TestContext.Current.CancellationToken);
            }
            var anonymous = _factory.CreateClient();

            var firstPage = await GetReviewsAsync(anonymous, movieId, "?page=1&pageSize=2");
            var secondPage = await GetReviewsAsync(anonymous, movieId, "?page=2&pageSize=2");

            firstPage.TotalReviews.Should().Be(3);
            firstPage.Items.Select(i => i.Rating).Should().Equal(3, 2);
            secondPage.Items.Select(i => i.Rating).Should().Equal(1);
        }

        [Theory]
        [InlineData("?page=0")]
        [InlineData("?pageSize=0")]
        [InlineData("?pageSize=51")]
        public async Task GetReviews_InvalidPaging_ReturnsBadRequest(string query)
        {
            var response = await _factory.CreateClient().GetAsync($"/api/movies/{NewMovieId()}/reviews{query}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReviews_InvalidMovieId_ReturnsNotFound()
        {
            var response = await _factory.CreateClient().GetAsync("/api/movies/0/reviews", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMyReview_ReturnsNotFoundUntilReviewExists()
        {
            var movieId = NewMovieId();
            var client = await CreateAuthenticatedClientAsync();

            var before = await client.GetAsync(MyReviewUrl(movieId), TestContext.Current.CancellationToken);
            await client.PutAsJsonAsync(MyReviewUrl(movieId), new UpsertReviewRequest(6, "ok"), cancellationToken: TestContext.Current.CancellationToken);
            var after = await client.GetAsync(MyReviewUrl(movieId), TestContext.Current.CancellationToken);

            before.StatusCode.Should().Be(HttpStatusCode.NotFound);
            after.StatusCode.Should().Be(HttpStatusCode.OK);
            (await after.Content.ReadFromJsonAsync<ReviewResponse>(cancellationToken: TestContext.Current.CancellationToken))!.Rating.Should().Be(6);
        }
    }
}
