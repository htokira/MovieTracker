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
    public class FollowsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public FollowsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<(HttpClient Client, Guid UserId, string Name)> CreateUserAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var email = $"{Guid.NewGuid():N}@example.com";
            var name = "Tester" + Guid.NewGuid().ToString("N")[..6];
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                DisplayName = name
            };

            var result = await users.CreateAsync(user, "Password123");
            result.Succeeded.Should().BeTrue();

            var (token, _) = tokens.CreateAccessToken(user);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return (client, user.Id, name);
        }

        [Fact]
        public async Task Follow_WithoutToken_ReturnsUnauthorized()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            var response = await client.PostAsync($"/api/users/{Guid.NewGuid()}/follow", null, ct);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Follow_Self_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, userId, _) = await CreateUserAsync();

            var response = await client.PostAsync($"/api/users/{userId}/follow", null, ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Follow_UnknownUser_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var response = await client.PostAsync($"/api/users/{Guid.NewGuid()}/follow", null, ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Follow_AddsFollowerAndFollowing()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, followerId, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();

            var response = await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var followers = await follower.GetFromJsonAsync<List<FollowUserResponse>>(
                $"/api/users/{targetId}/followers", ct);
            followers.Should().ContainSingle(f => f.Id == followerId);

            var following = await follower.GetFromJsonAsync<List<FollowUserResponse>>(
                $"/api/users/{followerId}/following", ct);
            following.Should().ContainSingle(f => f.Id == targetId);
        }

        [Fact]
        public async Task Follow_Twice_ReturnsNoContentAndKeepsSingleFollow()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();

            (await follower.PostAsync($"/api/users/{targetId}/follow", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await follower.PostAsync($"/api/users/{targetId}/follow", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            var followers = await follower.GetFromJsonAsync<List<FollowUserResponse>>(
                $"/api/users/{targetId}/followers", ct);
            followers.Should().HaveCount(1);
        }

        [Fact]
        public async Task Unfollow_WhenNotFollowing_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();

            var response = await client.DeleteAsync($"/api/users/{targetId}/follow", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Unfollow_RemovesFollow()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();
            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);

            var response = await follower.DeleteAsync($"/api/users/{targetId}/follow", ct);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var followers = await follower.GetFromJsonAsync<List<FollowUserResponse>>(
                $"/api/users/{targetId}/followers", ct);
            followers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFollowers_ReturnsNewestFirst()
        {
            var ct = TestContext.Current.CancellationToken;
            var (first, firstId, _) = await CreateUserAsync();
            var (second, secondId, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();

            await first.PostAsync($"/api/users/{targetId}/follow", null, ct);
            await Task.Delay(20, ct);
            await second.PostAsync($"/api/users/{targetId}/follow", null, ct);

            var followers = await first.GetFromJsonAsync<List<FollowUserResponse>>(
                $"/api/users/{targetId}/followers", ct);

            followers!.Select(f => f.Id).Should().Equal(secondId, firstId);
        }

        [Fact]
        public async Task GetFollowersAndFollowing_UnknownUser_ReturnNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();
            var unknown = Guid.NewGuid();

            (await client.GetAsync($"/api/users/{unknown}/followers", ct))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await client.GetAsync($"/api/users/{unknown}/following", ct))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetStats_ReturnsCountsAndIsFollowedByMe()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();
            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);

            var stats = await follower.GetFromJsonAsync<FollowStatsResponse>(
                $"/api/users/{targetId}/follow-stats", ct);

            stats!.FollowersCount.Should().Be(1);
            stats.FollowingCount.Should().Be(0);
            stats.IsFollowedByMe.Should().BeTrue();
        }

        [Fact]
        public async Task GetStats_Anonymous_IsFollowedByMeIsFalse()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, _) = await CreateUserAsync();
            var (_, targetId, _) = await CreateUserAsync();
            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);

            var anonymous = _factory.CreateClient();
            var stats = await anonymous.GetFromJsonAsync<FollowStatsResponse>(
                $"/api/users/{targetId}/follow-stats", ct);

            stats!.FollowersCount.Should().Be(1);
            stats.IsFollowedByMe.Should().BeFalse();
        }

        [Fact]
        public async Task GetStats_UnknownUser_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}/follow-stats", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetFeed_WithoutToken_ReturnsUnauthorized()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/users/me/feed", ct);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetFeed_WithNoFollows_ReturnsEmptyPage()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var feed = await client.GetFromJsonAsync<PagedResultDto<FeedItemResponse>>(
                "/api/users/me/feed", ct);

            feed!.Items.Should().BeEmpty();
            feed.TotalResults.Should().Be(0);
        }
    }
}
