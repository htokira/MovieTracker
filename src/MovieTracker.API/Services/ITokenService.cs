using MovieTracker.Core.Models;

namespace MovieTracker.API.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateAccessToken(User user);
        string GenerateRefreshToken();
        string HashToken(string token);
    }
}
