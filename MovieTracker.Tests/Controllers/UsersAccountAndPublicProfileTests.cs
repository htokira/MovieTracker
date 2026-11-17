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
    public class UsersAccountAndPublicProfileTests : IClassFixture<TestWebApplicationFactory>
    {
        private const string Password = "Password123";

        private readonly TestWebApplicationFactory _factory;

        public UsersAccountAndPublicProfileTests(TestWebApplicationFactory factory)
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

            var result = await users.CreateAsync(user, Password);
            result.Succeeded.Should().BeTrue();

            var (token, _) = tokens.CreateAccessToken(user);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return (client, user.Id, name);
        }

        // ---------- GET /api/users/{id} (публічний профіль) ----------

        [Fact]
        public async Task GetUserById_Anonymous_ReturnsPublicProfile()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId, name) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var profile = await anonymous.GetFromJsonAsync<PublicUserProfileResponse>(
                $"/api/users/{userId}", ct);

            profile!.Id.Should().Be(userId);
            profile.DisplayName.Should().Be(name);
        }

        [Fact]
        public async Task GetUserById_DoesNotExposeEmail()
        {
            var ct = TestContext.Current.CancellationToken;
            var (_, userId, _) = await CreateUserAsync();
            var anonymous = _factory.CreateClient();

            var json = await anonymous.GetStringAsync($"/api/users/{userId}", ct);

            json.Should().NotContainEquivalentOf("email");
        }

        [Fact]
        public async Task GetUserById_UnknownUser_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;
            var anonymous = _factory.CreateClient();

            var response = await anonymous.GetAsync($"/api/users/{Guid.NewGuid()}", ct);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ---------- PUT /api/users/me ----------

        [Fact]
        public async Task UpdateProfile_TrimsDisplayNameAndBio()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, userId, _) = await CreateUserAsync();

            var response = await client.PutAsJsonAsync("/api/users/me",
                new UpdateProfileRequest("  New Name  ", null, "  my bio  "), ct);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var profile = await client.GetFromJsonAsync<PublicUserProfileResponse>(
                $"/api/users/{userId}", ct);
            profile!.DisplayName.Should().Be("New Name");
            profile.Bio.Should().Be("my bio");
        }

        [Fact]
        public async Task UpdateProfile_BlankBio_IsStoredAsNull()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, userId, _) = await CreateUserAsync();
            await client.PutAsJsonAsync("/api/users/me",
                new UpdateProfileRequest("Some Name", null, "has bio"), ct);

            await client.PutAsJsonAsync("/api/users/me",
                new UpdateProfileRequest("Some Name", null, "   "), ct);

            var profile = await client.GetFromJsonAsync<PublicUserProfileResponse>(
                $"/api/users/{userId}", ct);
            profile!.Bio.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProfile_TooShortDisplayName_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var response = await client.PutAsJsonAsync("/api/users/me",
                new UpdateProfileRequest("A", null, null), ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateProfile_InvalidAvatarUrl_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var response = await client.PutAsJsonAsync("/api/users/me",
                new UpdateProfileRequest("Some Name", "not-a-url", null), ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ---------- POST /api/users/me/change-password ----------

        [Fact]
        public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/users/me/change-password",
                new ChangePasswordRequest(Password, "NewPassword123"), ct);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var response = await client.PostAsJsonAsync("/api/users/me/change-password",
                new ChangePasswordRequest("WrongPassword1", "NewPassword123"), ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ChangePassword_TooShortNewPassword_ReturnsBadRequest()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var response = await client.PostAsJsonAsync("/api/users/me/change-password",
                new ChangePasswordRequest(Password, "short"), ct);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ChangePassword_Valid_ReturnsNoContentAndOldPasswordStopsWorking()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, _, _) = await CreateUserAsync();

            var first = await client.PostAsJsonAsync("/api/users/me/change-password",
                new ChangePasswordRequest(Password, "NewPassword123"), ct);
            first.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var second = await client.PostAsJsonAsync("/api/users/me/change-password",
                new ChangePasswordRequest(Password, "AnotherPassword123"), ct);
            second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ---------- DELETE /api/users/me ----------

        [Fact]
        public async Task DeleteAccount_WithoutToken_ReturnsUnauthorized()
        {
            var ct = TestContext.Current.CancellationToken;
            var client = _factory.CreateClient();

            var response = await client.DeleteAsync("/api/users/me", ct);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAccount_RemovesUser()
        {
            var ct = TestContext.Current.CancellationToken;
            var (client, userId, _) = await CreateUserAsync();

            var response = await client.DeleteAsync("/api/users/me", ct);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var anonymous = _factory.CreateClient();
            (await anonymous.GetAsync($"/api/users/{userId}", ct))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
