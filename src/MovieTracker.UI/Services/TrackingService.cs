using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

public class TrackingService : ITrackingService
{
    private readonly AuthService _auth;

    // Приймає статус і як рядок, і як число
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackingService(AuthService auth)
    {
        _auth = auth;
    }

    public async Task<(List<TrackedMovieItem>? Items, string? Error)> GetMyMoviesAsync(WatchStatus status)
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Get, $"api/users/me/movies?status={status}&page=1&pageSize=50"));

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<UserMoviesResponse>(JsonOptions);
            if (result is null)
                return (null, "Сервер повернув пусту відповідь");

            return (result.Items.Select(TrackedMovieItem.From).ToList(), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, AuthService.NetworkError);
        }
        catch (JsonException)
        {
            return (null, "Не вдалося прочитати відповідь сервера.");
        }
    }

    public Task<string?> CreateCollectionAsync(string name, string? description, bool isPublic) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Post, "api/collections")
            {
                Content = JsonContent.Create(new CreateCollectionRequest(name, description, isPublic))
            });

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });

    // ===== Статус фільму =====

    public async Task<(WatchStatus? Status, string? Error)> GetMovieStatusAsync(int movieId)
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Get, $"api/movies/{movieId}/status"));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<WatchStatusResponse>(JsonOptions);
            return (result?.Status, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, AuthService.NetworkError);
        }
        catch (JsonException)
        {
            return (null, "Не вдалося прочитати відповідь сервера.");
        }
    }

    public Task<string?> SetMovieStatusAsync(int movieId, WatchStatus status) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Put, $"api/movies/{movieId}/status")
            {
                Content = JsonContent.Create(new SetWatchStatusRequest(status))
            });

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });

    public Task<string?> RemoveMovieStatusAsync(int movieId) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Delete, $"api/movies/{movieId}/status"));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });

    // ===== Колекції =====

    public async Task<(List<CollectionListItem>? Items, string? Error)> GetMyCollectionsAsync()
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Get, "api/collections/me"));

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<List<CollectionSummaryResponse>>(JsonOptions);
            if (result is null)
                return (null, "Сервер повернув пусту відповідь");

            return (result.Select(CollectionListItem.From).ToList(), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, AuthService.NetworkError);
        }
        catch (JsonException)
        {
            return (null, "Не вдалося прочитати відповідь сервера.");
        }
    }

    public async Task<(CollectionDetail? Detail, string? Error)> GetCollectionAsync(Guid collectionId)
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Get, $"api/collections/{collectionId}"));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return (null, "Колекцію не знайдено.");

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            var dto = await response.Content.ReadFromJsonAsync<CollectionDetailResponse>(JsonOptions);
            if (dto is null)
                return (null, "Сервер повернув пусту відповідь");

            return (CollectionDetail.From(dto), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, AuthService.NetworkError);
        }
        catch (JsonException)
        {
            return (null, "Не вдалося прочитати відповідь сервера.");
        }
    }

    public Task<string?> AddMovieToCollectionAsync(Guid collectionId, int movieId) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Post, $"api/collections/{collectionId}/movies/{movieId}"));

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });

    public Task<string?> RemoveMovieFromCollectionAsync(Guid collectionId, int movieId) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Delete, $"api/collections/{collectionId}/movies/{movieId}"));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });

    public Task<string?> DeleteCollectionAsync(Guid collectionId) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Delete, $"api/collections/{collectionId}"));

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });
}