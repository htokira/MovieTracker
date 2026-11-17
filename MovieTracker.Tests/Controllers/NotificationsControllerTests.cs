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
    public class NotificationsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public NotificationsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // Новий користувач з унікальним іменем і клієнт з його токеном
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

        private static Task<PagedResultDto<NotificationResponse>?> GetMineAsync(
            HttpClient client, CancellationToken ct, string query = "") =>
            client.GetFromJsonAsync<PagedResultDto<NotificationResponse>>($"/api/notifications/me{query}", ct);

        [Fact]
        public async Task AllEndpoints_WithoutToken_ReturnUnauthorized()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            (await client.GetAsync("/api/notifications/me", ct)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.GetAsync($"/api/notifications/{Guid.NewGuid()}", ct)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.PostAsync($"/api/notifications/{Guid.NewGuid()}/read", null, ct)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await client.PostAsync("/api/notifications/read-all", null, ct)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Follow_CreatesNotificationForFollowedUserOnly()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, followerId, followerName) = await CreateUserAsync();
            var (target, targetId, _) = await CreateUserAsync();

            (await follower.PostAsync($"/api/users/{targetId}/follow", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            var targetList = await GetMineAsync(target, ct);
            targetList!.TotalResults.Should().Be(1);
            var item = targetList.Items.Single();
            item.Type.Should().Be("NewFollower");
            item.Text.Should().Contain(followerName);
            item.ActorId.Should().Be(followerId);
            item.IsRead.Should().BeFalse();

            (await GetMineAsync(follower, ct))!.TotalResults.Should().Be(0);
        }

        [Fact]
        public async Task Follow_Twice_CreatesSingleNotification()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, _) = await CreateUserAsync();
            var (target, targetId, _) = await CreateUserAsync();

            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);
            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);

            (await GetMineAsync(target, ct))!.TotalResults.Should().Be(1);
        }

        [Fact]
        public async Task Follow_Yourself_DoesNotCreateNotification()
        {
            var ct = TestContext.Current.CancellationToken;
            var (user, userId, _) = await CreateUserAsync();

            (await user.PostAsync($"/api/users/{userId}/follow", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await GetMineAsync(user, ct))!.TotalResults.Should().Be(0);
        }

        [Fact]
        public async Task GetById_ReturnsDetail_OnlyForOwner()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, followerName) = await CreateUserAsync();
            var (target, targetId, _) = await CreateUserAsync();
            var (stranger, _, _) = await CreateUserAsync();
            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);
            var id = (await GetMineAsync(target, ct))!.Items.Single().Id;

            var own = await target.GetAsync($"/api/notifications/{id}", ct);
            own.StatusCode.Should().Be(HttpStatusCode.OK);
            (await own.Content.ReadFromJsonAsync<NotificationResponse>(ct))!.Text.Should().Contain(followerName);

            (await stranger.GetAsync($"/api/notifications/{id}", ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await target.GetAsync($"/api/notifications/{Guid.NewGuid()}", ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task MarkAsRead_MarksOnlyOwnNotification()
        {
            var ct = TestContext.Current.CancellationToken;
            var (follower, _, _) = await CreateUserAsync();
            var (target, targetId, _) = await CreateUserAsync();
            var (stranger, _, _) = await CreateUserAsync();
            await follower.PostAsync($"/api/users/{targetId}/follow", null, ct);
            var id = (await GetMineAsync(target, ct))!.Items.Single().Id;

            (await stranger.PostAsync($"/api/notifications/{id}/read", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await target.PostAsync($"/api/notifications/{id}/read", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            var detail = await target.GetFromJsonAsync<NotificationResponse>($"/api/notifications/{id}", ct);
            detail!.IsRead.Should().BeTrue();
        }

        [Fact]
        public async Task MarkAllAsRead_MarksEverything_AndUnreadFilterWorks()
        {
            var ct = TestContext.Current.CancellationToken;
            var (first, _, _) = await CreateUserAsync();
            var (second, _, _) = await CreateUserAsync();
            var (target, targetId, _) = await CreateUserAsync();
            await first.PostAsync($"/api/users/{targetId}/follow", null, ct);
            await second.PostAsync($"/api/users/{targetId}/follow", null, ct);

            (await GetMineAsync(target, ct, "?unreadOnly=true"))!.TotalResults.Should().Be(2);

            (await target.PostAsync("/api/notifications/read-all", null, ct))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            (await GetMineAsync(target, ct, "?unreadOnly=true"))!.TotalResults.Should().Be(0);
            (await GetMineAsync(target, ct))!.TotalResults.Should().Be(2);
        }
    }
}
