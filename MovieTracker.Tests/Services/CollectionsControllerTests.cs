using FluentAssertions;
using MovieTracker.Core.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MovieTracker.Tests.Services
{
    public class CollectionsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private static int _lastMovieId = 5000;
        private readonly TestWebApplicationFactory _factory;

        public CollectionsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private static int NewMovieId() => Interlocked.Increment(ref _lastMovieId);

        private async Task<(HttpClient Client, Guid UserId)> CreateAuthenticatedClientAsync(string displayName = "Collector")
        {
            var client = _factory.CreateClient();
            var request = new RegisterRequest($"user_{Guid.NewGuid():N}@example.com", "StrongPassword123!", displayName);

            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            response.EnsureSuccessStatusCode();

            var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

            var me = await client.GetFromJsonAsync<UserProfileResponse>("/api/users/me");
            return (client, me!.Id);
        }

        private static async Task<CollectionSummaryResponse> CreateCollectionAsync(
            HttpClient client, string name = "My list", bool isPublic = true, string? description = null)
        {
            var response = await client.PostAsJsonAsync("/api/collections",
                new CreateCollectionRequest(name, description, isPublic));
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await response.Content.ReadFromJsonAsync<CollectionSummaryResponse>())!;
        }

        // ---------- Створення ----------

        [Fact]
        public async Task Create_ReturnsCollectionWithTrimmedName()
        {
            var (client, userId) = await CreateAuthenticatedClientAsync("Alice");

            var collection = await CreateCollectionAsync(client, "  Favorites  ", true, "  desc  ");

            collection.Name.Should().Be("Favorites");
            collection.Description.Should().Be("desc");
            collection.OwnerName.Should().Be("Alice");
            collection.UserId.Should().Be(userId);
            collection.MoviesCount.Should().Be(0);
        }

        [Theory]
        [InlineData("")]
        public async Task Create_EmptyName_ReturnsBadRequest(string name)
        {
            var (client, _) = await CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/collections",
                new CreateCollectionRequest(name, null, true), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_NameTooLong_ReturnsBadRequest()
        {
            var (client, _) = await CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/collections",
                new CreateCollectionRequest(new string('a', 101), null, true), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_WithoutToken_ReturnsUnauthorized()
        {
            var anonymous = _factory.CreateClient();

            var response = await anonymous.PostAsJsonAsync("/api/collections",
                new CreateCollectionRequest("x", null, true), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ---------- Мої колекції ----------

        [Fact]
        public async Task GetMine_ReturnsOnlyOwnCollections_PublicAndPrivate()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var (other, _) = await CreateAuthenticatedClientAsync();
            await CreateCollectionAsync(owner, "Public", true);
            await CreateCollectionAsync(owner, "Private", false);
            await CreateCollectionAsync(other, "Not mine", true);

            var mine = await owner.GetFromJsonAsync<List<CollectionSummaryResponse>>(
                "/api/collections/me", TestContext.Current.CancellationToken);

            mine!.Select(c => c.Name).Should().BeEquivalentTo(new[] { "Public", "Private" });
        }

        // ---------- Приватність ----------

        [Fact]
        public async Task GetById_PublicCollection_VisibleToAnonymous()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(owner, "Open", true);

            var response = await _factory.CreateClient()
                .GetAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_PrivateCollection_NotFoundForAnonymousAndStranger()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var (stranger, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(owner, "Secret", false);

            var anonymous = await _factory.CreateClient()
                .GetAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken);
            var asStranger = await stranger
                .GetAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken);

            anonymous.StatusCode.Should().Be(HttpStatusCode.NotFound);
            asStranger.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetById_PrivateCollection_VisibleToOwner()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(owner, "Secret", false);

            var response = await owner
                .GetAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_UnknownId_ReturnsNotFound()
        {
            var response = await _factory.CreateClient()
                .GetAsync($"/api/collections/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ---------- Оновлення і видалення ----------

        [Fact]
        public async Task Update_ChangesFields()
        {
            var (client, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(client, "Old", true);

            var response = await client.PutAsJsonAsync($"/api/collections/{created.Id}",
                new UpdateCollectionRequest("New", "new desc", false), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<CollectionSummaryResponse>(
                cancellationToken: TestContext.Current.CancellationToken);
            updated!.Name.Should().Be("New");
            updated.Description.Should().Be("new desc");
            updated.IsPublic.Should().BeFalse();
        }

        [Fact]
        public async Task Update_OtherUsersCollection_ReturnsNotFound()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var (stranger, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(owner, "Mine", true);

            var response = await stranger.PutAsJsonAsync($"/api/collections/{created.Id}",
                new UpdateCollectionRequest("Hacked", null, true), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_RemovesOwnCollection()
        {
            var (client, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(client);

            var response = await client.DeleteAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await client.GetAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_OtherUsersCollection_ReturnsNotFoundAndKeepsIt()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var (stranger, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(owner, "Mine", true);

            var response = await stranger.DeleteAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await owner.GetAsync($"/api/collections/{created.Id}", TestContext.Current.CancellationToken))
                .StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ---------- Фільми в колекції ----------

        [Fact]
        public async Task AddMovie_AppearsInCollection_AndIsNotDuplicatedOnSecondAdd()
        {
            var (client, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(client);
            var movieId = NewMovieId();

            var first = await client.PostAsync($"/api/collections/{created.Id}/movies/{movieId}", null, TestContext.Current.CancellationToken);
            var second = await client.PostAsync($"/api/collections/{created.Id}/movies/{movieId}", null, TestContext.Current.CancellationToken);

            first.StatusCode.Should().Be(HttpStatusCode.NoContent);
            second.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var detail = await client.GetFromJsonAsync<CollectionDetailResponse>(
                $"/api/collections/{created.Id}", TestContext.Current.CancellationToken);
            detail!.Movies.Should().ContainSingle(m => m.MovieId == movieId);
        }

        [Fact]
        public async Task AddMovie_ToOtherUsersCollection_ReturnsNotFound()
        {
            var (owner, _) = await CreateAuthenticatedClientAsync();
            var (stranger, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(owner);

            var response = await stranger.PostAsync(
                $"/api/collections/{created.Id}/movies/{NewMovieId()}", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RemoveMovie_RemovesItFromCollection()
        {
            var (client, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(client);
            var movieId = NewMovieId();
            await client.PostAsync($"/api/collections/{created.Id}/movies/{movieId}", null, TestContext.Current.CancellationToken);

            var response = await client.DeleteAsync(
                $"/api/collections/{created.Id}/movies/{movieId}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var detail = await client.GetFromJsonAsync<CollectionDetailResponse>(
                $"/api/collections/{created.Id}", TestContext.Current.CancellationToken);
            detail!.Movies.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveMovie_NotInCollection_ReturnsNotFound()
        {
            var (client, _) = await CreateAuthenticatedClientAsync();
            var created = await CreateCollectionAsync(client);

            var response = await client.DeleteAsync(
                $"/api/collections/{created.Id}/movies/{NewMovieId()}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ---------- Колекції іншого користувача ----------

        [Fact]
        public async Task GetUserPublicCollections_ReturnsOnlyPublic()
        {
            var (owner, ownerId) = await CreateAuthenticatedClientAsync();
            await CreateCollectionAsync(owner, "Public one", true);
            await CreateCollectionAsync(owner, "Private one", false);

            var items = await _factory.CreateClient().GetFromJsonAsync<List<CollectionSummaryResponse>>(
                $"/api/users/{ownerId}/collections", TestContext.Current.CancellationToken);

            items!.Select(c => c.Name).Should().Equal("Public one");
        }
    }
}
