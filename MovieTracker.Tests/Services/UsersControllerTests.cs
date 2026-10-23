using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieTracker.Core.Dtos;
using MovieTracker.Data;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace MovieTracker.Tests.Services
{
    public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UsersControllerTests(WebApplicationFactory<Program> factory)
        {
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "Jwt:Key", "PipelineplsWorkPipelineplsWorkPipelineplsWorkPipelinepls" },
                        { "Jwt:Issuer", "TestIssuer" },
                        { "Jwt:Audience", "TestAudience" }
                    });
                });

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<MovieTrackerContext>));
                    services.AddDbContext<MovieTrackerContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestUsersDb");
                    });
                });
            });

            _client = customFactory.CreateClient();
        }

        private async Task AuthenticateClientAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            var request = new RegisterRequest(email, "StrongPassword123!", "Test User");
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        }

        [Fact]
        public async Task GetProfile_WhenAuthenticated_ReturnsUserProfile()
        {
            await AuthenticateClientAsync();

            var response = await _client.GetAsync("/api/users/me");

            response.IsSuccessStatusCode.Should().BeTrue();
            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            profile.Should().NotBeNull();
            profile!.Email.Should().Contain("@example.com");
            profile.DisplayName.Should().Be("Test User");
        }

        [Fact]
        public async Task UpdateProfile_WhenAuthenticated_UpdatesData()
        {
            await AuthenticateClientAsync();
            var updateReq = new UpdateProfileRequest("Updated Name", "https://example.com/avatar.jpg", "New Bio");

            var response = await _client.PutAsJsonAsync("/api/users/me", updateReq);

            response.IsSuccessStatusCode.Should().BeTrue();
            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            profile.Should().NotBeNull();
            profile!.DisplayName.Should().Be("Updated Name");
            profile.Bio.Should().Be("New Bio");
            profile.AvatarUrl.Should().Be("https://example.com/avatar.jpg");
        }

        [Fact]
        public async Task GetProfile_WhenNotAuthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}