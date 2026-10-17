using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MovieTracker.API.Services;
using MovieTracker.API.Settings;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _users;
        private readonly MovieTrackerContext _db;
        private readonly ITokenService _tokens;
        private readonly JwtSettings _jwt;

        public AuthController(UserManager<User> users, MovieTrackerContext db, ITokenService tokens, IOptions<JwtSettings> jwt)
        {
            _users = users;
            _db = db;
            _tokens = tokens;
            _jwt = jwt.Value;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                DisplayName = request.DisplayName.Trim()
            };

            var result = await _users.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
                return ValidationProblem(ModelState);
            }

            return Ok(await IssueTokensAsync(user));
        }

        private async Task<AuthResponse> IssueTokensAsync(User user)
        {
            var (accessToken, accessExpires) = _tokens.CreateAccessToken(user);
            var refreshToken = _tokens.GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = _tokens.HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
            });
            await _db.SaveChangesAsync();

            return new AuthResponse(accessToken, accessExpires, refreshToken);
        }
    }
}
