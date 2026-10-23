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
using System.Net.Http.Json;
using System.Text;

namespace MovieTracker.Tests.Services
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
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
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerReq);

            var registerBody = await registerResponse.Content.ReadAsStringAsync();

            registerResponse.IsSuccessStatusCode.Should().BeTrue(
                $"registration failed: {(int)registerResponse.StatusCode} {registerBody}");

            var loginReq = new LoginRequest("loginuser@example.com", "StrongPassword123!");

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginReq);

            var body = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue($"Login returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Refresh_WithValidToken_ReturnsNewTokens()
        {
            var registerReq = new RegisterRequest("refreshuser@example.com", "StrongPassword123!", "Refresh User");
            var regRes = await _client.PostAsJsonAsync( "/api/auth/register", registerReq);

            var regBody = await regRes.Content.ReadAsStringAsync();

            regRes.IsSuccessStatusCode.Should().BeTrue(
                $"registration failed: {(int)regRes.StatusCode} {regBody}");

            var tokens = await regRes.Content.ReadFromJsonAsync<AuthResponse>();

            tokens.Should().NotBeNull();

            var refreshReq = new RefreshRequest(tokens!.RefreshToken);

            var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);

            var body = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue($"Refresh returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
            var newTokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
            newTokens.Should().NotBeNull();
            newTokens!.AccessToken.Should().NotBeNullOrEmpty();
            newTokens.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Logout_WithValidToken_ReturnsNoContent()
        {
            var registerReq = new RegisterRequest("logoutuser@example.com", "StrongPassword123!", "Logout User");
            var regRes = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
            var tokens = await regRes.Content.ReadFromJsonAsync<AuthResponse>();

            var refreshReq = new RefreshRequest(tokens!.RefreshToken);

            var response = await _client.PostAsJsonAsync("/api/auth/logout", refreshReq);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }
    }
}