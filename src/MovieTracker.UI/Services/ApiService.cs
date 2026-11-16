using System.Net.Http.Json;
using MovieTracker.Core.Dtos;
using System.Net;

namespace MovieTracker.UI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _auth;

    public ApiService(HttpClient httpClient, AuthService auth)
    {
        _httpClient = httpClient;
        _auth = auth;
    }

    public async Task<PagedResultDto<MovieListItemDto>> GetPopularMoviesAsync(int page = 1)
    {
        var movies = await _httpClient.GetFromJsonAsync<PagedResultDto<MovieListItemDto>>(
            $"api/movies/popular?page={page}");

        return movies ?? new PagedResultDto<MovieListItemDto>();
    }

    public async Task<PagedResultDto<MovieListItemDto>> GetMoviesAsync(string? query = null, int page = 1,
    int? genreId = null, string? sortBy = null)
    {
        var parameters = new List<string>
        {
            $"page={page}"
        };

        if (!string.IsNullOrEmpty(query))
        {
            parameters.Add($"query={query}");
        }

        if (genreId.HasValue)
        {
            parameters.Add($"genreId={genreId.Value}");
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            parameters.Add($"sortBy={sortBy}");
        }

        var url = $"api/movies?{string.Join("&", parameters)}";
        var result = await _httpClient.GetFromJsonAsync<PagedResultDto<MovieListItemDto>>(url);
        return result ?? new PagedResultDto<MovieListItemDto>();
    }

    public async Task<MovieDetailDto?> GetMovieDetailsAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MovieDetailDto>($"api/movies/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<MovieReviewsResponse?> GetMovieReviewsAsync(
    int movieId,
    int page = 1,
    int pageSize = 10)
    {
        return await _httpClient.GetFromJsonAsync<MovieReviewsResponse>(
            $"api/movies/{movieId}/reviews?page={page}&pageSize={pageSize}");
    }

    private static string MyReviewUrl(int movieId) => $"api/movies/{movieId}/reviews/me";

    public async Task<(ReviewResponse? Review, string? Error)> GetMyReviewAsync(int movieId)
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(
                () => new HttpRequestMessage(HttpMethod.Get, MyReviewUrl(movieId)));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            return (await response.Content.ReadFromJsonAsync<ReviewResponse>(), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, AuthService.NetworkError);
        }
    }

    public Task<string?> SaveMyReviewAsync(int movieId, int rating, string? text) => AuthService.GuardAsync(async () =>
    {
        var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Put, MyReviewUrl(movieId))
        {
            Content = JsonContent.Create(new UpsertReviewRequest(rating, text))
        });

        if (!response.IsSuccessStatusCode)
        {
            return await AuthService.ReadErrorAsync(response);
        }

        await AnalyzeTasteProfileAsync();
        return null;
    });

    public Task<string?> DeleteMyReviewAsync(int movieId) => AuthService.GuardAsync(async () =>
    {
        var response = await _auth.SendAuthorizedAsync(
            () => new HttpRequestMessage(HttpMethod.Delete, MyReviewUrl(movieId)));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return await AuthService.ReadErrorAsync(response);
        }

        await AnalyzeTasteProfileAsync();
        return null;
    });

    private const string MyRecommendationsUrl = "api/recommendations/me";
    public async Task<(RecommendationsResponse? Response, string? Error)> GetMyRecommendationsAsync()
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(
                () => new HttpRequestMessage(HttpMethod.Get, MyRecommendationsUrl));

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            return (await response.Content.ReadFromJsonAsync<RecommendationsResponse>(), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, AuthService.NetworkError);
        }
    }

    public async Task<TasteProfileDto?> GetTasteProfileAsync()
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(
                () => new HttpRequestMessage(HttpMethod.Get, "api/taste-profile/me"));

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TasteProfileDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<TasteProfileDto?> AnalyzeTasteProfileAsync()
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(
                () => new HttpRequestMessage(HttpMethod.Post, "api/taste-profile/analyze"));

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TasteProfileDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }
        // ===== Профіль іншого користувача =====

    public async Task<PublicUserProfileResponse?> GetUserProfileAsync(Guid userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PublicUserProfileResponse>(
                $"api/users/{userId}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                       or System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<FollowStatsResponse?> GetFollowStatsAsync(Guid userId)
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(
                () => new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}/follow-stats"));

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<FollowStatsResponse>();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                       or System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public Task<string?> FollowUserAsync(Guid userId) => AuthService.GuardAsync(async () =>
    {
        var response = await _auth.SendAuthorizedAsync(
            () => new HttpRequestMessage(HttpMethod.Post, $"api/users/{userId}/follow"));

        return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
    });

    public Task<string?> UnfollowUserAsync(Guid userId) => AuthService.GuardAsync(async () =>
    {
        var response = await _auth.SendAuthorizedAsync(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/users/{userId}/follow"));

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null; // вже не підписана, це не помилка

        return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
    });
        public async Task<UserReviewsResponse?> GetUserReviewsAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserReviewsResponse>(
                $"api/users/{userId}/reviews?page={page}&pageSize={pageSize}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                       or System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<List<CollectionSummaryResponse>?> GetUserCollectionsAsync(Guid userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CollectionSummaryResponse>>(
                $"api/users/{userId}/collections");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                       or System.Text.Json.JsonException)
        {
            return null;
        }
    }
}
