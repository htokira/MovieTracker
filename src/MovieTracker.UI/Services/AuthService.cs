using MovieTracker.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MovieTracker.UI.Services
{
    public class AuthService
    {
        private const string RefreshTokenKey = "refresh_token";

        private readonly HttpClient _http;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private string? _refreshToken;

        private static readonly Dictionary<string, string> ErrorTranslations = new()
        {
            ["DuplicateEmail"] = "Акаунт із таким email уже існує.",
            ["DuplicateUserName"] = "Акаунт із таким email уже існує.",
            ["InvalidEmail"] = "Введіть коректний email.",
            ["PasswordTooShort"] = "Пароль має містити щонайменше 8 символів.",
            ["PasswordRequiresUpper"] = "Пароль має містити велику літеру.",
            ["PasswordRequiresLower"] = "Пароль має містити малу літеру.",
            ["PasswordRequiresDigit"] = "Пароль має містити цифру.",
            ["PasswordMismatch"] = "Невірний поточний пароль."
        };

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        private UserProfileResponse? _currentUser;

        public UserProfileResponse? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                AuthStateChanged?.Invoke();
            }
        }

        public event Action? AuthStateChanged;
        public bool IsAuthenticated => CurrentUser is not null;

        public async Task RestoreSessionAsync()
        {
            try
            {
                _refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            }
            catch (Exception)
            {
                _refreshToken = null;
            }

            if (string.IsNullOrEmpty(_refreshToken))
                return;

            try
            {
                if (await RefreshAsync())
                    await LoadProfileAsync();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // server unavaliable
            }
        }

        public Task<string?> RegisterAsync(string email, string password, string displayName) => GuardAsync(async () =>
        {
            var response = await _http.PostAsJsonAsync("api/auth/register",
                new RegisterRequest(email, password, displayName));
            return await HandleAuthResponseAsync(response);
        });

        public Task<string?> LoginAsync(string email, string password) => GuardAsync(async () =>
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return "Невірний email або пароль.";
            if (response.StatusCode == HttpStatusCode.Locked)
                return "Забагато невдалих спроб входу. Спробуйте пізніше.";

            return await HandleAuthResponseAsync(response);
        });

        public Task<string?> UpdateProfileAsync(string displayName, string? avatarUrl, string? bio) => GuardAsync(async () =>
        {
            var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Put, "api/users/me")
            {
                Content = JsonContent.Create(new UpdateProfileRequest(displayName, avatarUrl, bio))
            });
            if (!response.IsSuccessStatusCode)
                return await ReadErrorAsync(response);

            CurrentUser = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            return null;
        });

        public Task<string?> ChangePasswordAsync(string currentPassword, string newPassword) => GuardAsync(async () =>
        {
            var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, "api/users/me/change-password")
            {
                Content = JsonContent.Create(new ChangePasswordRequest(currentPassword, newPassword))
            });
            return response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);
        });

        public Task<string?> DeleteAccountAsync() => GuardAsync(async () =>
        {
            var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Delete, "api/users/me"));
            if (!response.IsSuccessStatusCode)
                return await ReadErrorAsync(response);

            ClearSession();
            return null;
        });

        public async Task LogoutAsync()
        {
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                try
                {
                    await _http.PostAsJsonAsync("api/auth/logout", new RefreshRequest(_refreshToken));
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                }
            }

            ClearSession();
        }

        private async Task<string?> HandleAuthResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return await ReadErrorAsync(response);

            var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (tokens is null)
                return "Сервер повернув пусту відповідь";

            await SetSessionAsync(tokens);
            return await LoadProfileAsync();
        }

        private async Task<string?> LoadProfileAsync()
        {
            var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Get, "api/users/me"));
            if (!response.IsSuccessStatusCode)
                return await ReadErrorAsync(response);

            CurrentUser = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            return null;
        }

        private async Task<HttpResponseMessage> SendAuthorizedAsync(Func<HttpRequestMessage> createRequest)
        {
            var response = await _http.SendAsync(createRequest());
            if (response.StatusCode != HttpStatusCode.Unauthorized || !await RefreshAsync())
                return response;

            response.Dispose();
            var retry = await _http.SendAsync(createRequest());
            if (retry.StatusCode == HttpStatusCode.Unauthorized)
                ClearSession();
            return retry;
        }

        private async Task<bool> RefreshAsync()
        {
            var usedToken = _refreshToken;
            if (string.IsNullOrEmpty(usedToken))
                return false;

            await _refreshLock.WaitAsync();
            try
            {
                if (_refreshToken != usedToken)
                    return !string.IsNullOrEmpty(_refreshToken);

                var response = await _http.PostAsJsonAsync("api/auth/refresh", new RefreshRequest(usedToken));
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        ClearSession();
                    return false;
                }

                var tokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (tokens is null)
                    return false;

                await SetSessionAsync(tokens);
                return true;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task SetSessionAsync(AuthResponse tokens)
        {
            _refreshToken = tokens.RefreshToken;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            try
            {
                await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
            }
            catch (Exception)
            {
                // SecureStorage unavaliable
            }
        }

        private void ClearSession()
        {
            _refreshToken = null;
            CurrentUser = null;
            _http.DefaultRequestHeaders.Authorization = null;

            try
            {
                SecureStorage.Default.Remove(RefreshTokenKey);
            }
            catch (Exception)
            {
                // nothing to clear
            }
        }

        private static async Task<string?> GuardAsync(Func<Task<string?>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return "Не вдалося зв’язатися із сервером. Перевірте, чи запущено API.";
            }
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray().Select(m =>
                            ErrorTranslations.TryGetValue(p.Name, out var translated) ? translated : m.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .Distinct();

                    var text = string.Join("\n", messages);
                    if (text.Length > 0)
                        return text;
                }

                if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                    return title.GetString()!;
            }
            catch (JsonException)
            {
            }

            return response.StatusCode == HttpStatusCode.Unauthorized
                ? "Сесія завершилась. Увійдіть знову."
                : $"Помилка сервера ({(int)response.StatusCode}).";
        }
    }
}