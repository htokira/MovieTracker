using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _users.FindByEmailAsync(request.Email.Trim());
            if (user is null)
                return InvalidCredentials();

            if (await _users.IsLockedOutAsync(user))
                return Problem(title: "Too many login tries, try again later.",
                    statusCode: StatusCodes.Status423Locked);

            if (!await _users.CheckPasswordAsync(user, request.Password))
            {
                await _users.AccessFailedAsync(user);
                return InvalidCredentials();
            }

            await _users.ResetAccessFailedCountAsync(user);
            return Ok(await IssueTokensAsync(user));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
        {
            var hash = _tokens.HashToken(request.RefreshToken);
            var stored = await _db.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored is null)
                return InvalidToken();

            if (stored.RevokedAt is not null)
            {
                await RevokeAllForUserAsync(stored.UserId);
                return InvalidToken();
            }

            if (!stored.IsActive)
                return InvalidToken();

            stored.RevokedAt = DateTime.UtcNow;
            return Ok(await IssueTokensAsync(stored.User));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshRequest request)
        {
            var hash = _tokens.HashToken(request.RefreshToken);
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null);

            if (stored is not null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return NoContent();
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

        private async Task RevokeAllForUserAsync(Guid userId)
        {
            var active = await _db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync();
            foreach (var token in active)
                token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private ObjectResult InvalidCredentials() => Problem(title: "Wrong email or password.", statusCode: StatusCodes.Status401Unauthorized);
        private ObjectResult InvalidToken() => Problem(title: "Invalid refresh token", statusCode: StatusCodes.Status401Unauthorized);
    }
}
