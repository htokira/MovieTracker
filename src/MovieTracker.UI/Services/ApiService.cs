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

    public async Task<IEnumerable<MovieListItemDto>> GetPopularMoviesAsync(int page = 1)
    {
        var movies = await _httpClient.GetFromJsonAsync<IEnumerable<MovieListItemDto>>(
            $"api/movies/popular?page={page}");

        return movies ?? Enumerable.Empty<MovieListItemDto>();
    }

    public async Task<IEnumerable<MovieListItemDto>> GetMoviesAsync(string? query = null, int page = 1, 
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
        var movies = await _httpClient.GetFromJsonAsync<IEnumerable<MovieListItemDto>>(url);
        return movies ?? Enumerable.Empty<MovieListItemDto>();
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

        return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
    });

    public Task<string?> DeleteMyReviewAsync(int movieId) => AuthService.GuardAsync(async () =>
    {
        var response = await _auth.SendAuthorizedAsync(
            () => new HttpRequestMessage(HttpMethod.Delete, MyReviewUrl(movieId)));

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await AuthService.ReadErrorAsync(response);
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
}