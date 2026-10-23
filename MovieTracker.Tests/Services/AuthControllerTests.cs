using Microsoft.AspNetCore.Mvc.Testing;
using MovieTracker.Core.Dtos;
using MovieTracker.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace MovieTracker.Tests.Services
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<MovieTrackerContext>));
                    services.AddDbContext<MovieTrackerContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestAuthDb");
                    });
                });
            });

            _client = customFactory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOkAndTokens()
        {
            var request = new RegisterRequest("newuser@example.com", "StrongPassword123!", "John");

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            response.IsSuccessStatusCode.Should().BeTrue();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithValidData_ReturnsOkAndTokens()
        {
            var registerReq = new RegisterRequest("loginuser@example.com", "StrongPassword123!", "Jane");
            await _client.PostAsJsonAsync("/api/auth/register", registerReq);

            var loginReq = new LoginRequest("loginuser@example.com", "StrongPassword123!");

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginReq);

            response.IsSuccessStatusCode.Should().BeTrue();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
        }
    }
}