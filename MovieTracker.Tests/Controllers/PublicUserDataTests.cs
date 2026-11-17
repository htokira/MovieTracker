using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MovieTracker.Tests.Controllers
{
    public class PublicUserDataTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public PublicUserDataTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

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

        // ---------- GET /api/users/{id}/reviews ----------

        [Fact]
        public async Task GetUserReviews_UnknownUser_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/users/{Guid.NewGuid()}/reviews", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserReviews_UserWithoutReviews_ReturnsEmptyPage()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var result = await anonymous.GetFromJsonAsync<UserReviewsResponse>(
                $"/api/users/{userId}/reviews", ct);

            result!.UserId.Should().Be(userId);
            result.TotalCount.Should().Be(0);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserReviews_PageZero_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/users/{userId}/reviews?page=0", ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserReviews_PageSizeAboveMax_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/users/{userId}/reviews?pageSize=51", ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ---------- GET /api/users/{id}/taste-profile ----------

        [Fact]
        public async Task GetUserTasteProfile_UnknownUser_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/users/{Guid.NewGuid()}/taste-profile", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserTasteProfile_UserWithoutProfile_ReturnsNotFoundForAnonymous()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/users/{userId}/taste-profile", ct);

            // 404, а не 401: ендпоінт публічний
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserTasteProfile_AliasRoute_BehavesTheSame()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/taste-profile/{userId}", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMyTasteProfile_WithoutToken_ReturnsUnauthorized()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/taste-profile/me", ct);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyTasteProfile_NoProfileYet_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _) = await CreateUserAsync();

            var response = await client.GetAsync("/api/taste-profile/me", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
