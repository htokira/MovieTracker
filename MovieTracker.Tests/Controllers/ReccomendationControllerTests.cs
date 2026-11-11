using FluentAssertions;
using MovieTracker.Core.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MovieTracker.Tests.Controllers;

public class RecommendationsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private const string MyRecommendationsUrl = "/api/recommendations/me";
    private readonly TestWebApplicationFactory _factory;

    public RecommendationsControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest($"rec_user_{Guid.NewGuid():N}@example.com", "Password123!", "RecTester");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetMyRecommendations_WithoutToken_ReturnsUnauthorized()
    {
        var anonymous = _factory.CreateClient();

        var response = await anonymous.GetAsync(MyRecommendationsUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyRecommendations_WithoutTasteProfile_ReturnsNonPersonalizedResult()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync(MyRecommendationsUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RecommendationsResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result!.IsPersonalized.Should().BeFalse();
        result.Items.Count.Should().BeLessThanOrEqualTo(20);
    }
}