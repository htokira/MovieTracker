using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MovieTracker.Core.Dtos;
using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

public class NotificationService : INotificationService
{
    private readonly AuthService _auth;

    public NotificationService(AuthService auth)
    {
        _auth = auth;
    }

    public async Task<(List<NotificationItem>? Items, string? Error)> GetNotificationsAsync()
    {
        try
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Get, "api/notifications/me?page=1&pageSize=50"));

            if (!response.IsSuccessStatusCode)
                return (null, await AuthService.ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<NotificationResponse>>();
            if (result is null)
                return (null, "Сервер повернув пусту відповідь");

            return (result.Items.Select(NotificationItem.From).ToList(), null);
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

    public Task<string?> MarkReadAsync(Guid notificationId) =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Post, $"api/notifications/{notificationId}/read"));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });

    public Task<string?> MarkAllReadAsync() =>
        AuthService.GuardAsync(async () =>
        {
            var response = await _auth.SendAuthorizedAsync(() => new HttpRequestMessage(
                HttpMethod.Post, "api/notifications/read-all"));

            return response.IsSuccessStatusCode ? null : await AuthService.ReadErrorAsync(response);
        });
}
