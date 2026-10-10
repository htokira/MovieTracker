using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MovieTracker.API.Settings;
using MovieTracker.Core.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MovieTracker.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public (string Token, DateTime ExpiresAt) CreateAccessToken(User user)
        {
            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", user.DisplayName)
            }),
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);
            return (token, expires);
        }

        public string GenerateRefreshToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
