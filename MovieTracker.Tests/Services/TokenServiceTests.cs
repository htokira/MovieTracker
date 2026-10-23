using Microsoft.Extensions.Options;
using MovieTracker.API.Services;
using MovieTracker.API.Settings;
using MovieTracker.Core.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieTracker.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly JwtSettings _settings;

        public TokenServiceTests()
        {
            _settings = new JwtSettings
            {
                Key = "keyKeyKey!!!IsItReallyAKeyOrJuSTabLuffIDOntkNOw67",
                Issuer = "MovieTrackerAPI",
                Audience = "MovieTrackerUI",
                ExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            };

            var options = Options.Create(_settings);
            _tokenService = new TokenService(options);
        }

        [Fact]
        public void CreateAccessToken_ReturnsValidTokenAndExpiration()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                DisplayName = "TestUser"
            };

            var result = _tokenService.CreateAccessToken(user);

            result.Token.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsBase64StringOf64Bytes()
        {
            var token = _tokenService.GenerateRefreshToken();

            token.Should().NotBeNullOrEmpty();
            Convert.FromBase64String(token).Length.Should().Be(64);
        }

        [Fact]
        public void HashToken_ReturnsConsistentHash()
        {
            var rawToken = "some_random_refresh_token_string";

            var hash1 = _tokenService.HashToken(rawToken);
            var hash2 = _tokenService.HashToken(rawToken);

            hash1.Should().NotBeNullOrEmpty();
            hash1.Should().Be(hash2);
        }
    }
}