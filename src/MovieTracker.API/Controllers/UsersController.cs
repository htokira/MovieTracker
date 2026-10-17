using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users/me")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _users;
        private readonly MovieTrackerContext _db;

        public UsersController(UserManager<User> users, MovieTrackerContext db)
        {
            _users = users;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<UserProfileResponse>> GetProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();
            return Ok(ToResponse(user));
        }

        [HttpPut]
        public async Task<ActionResult<UserProfileResponse>> UpdateProfile(UpdateProfileRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            user.DisplayName = request.DisplayName.Trim();
            user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
            user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();

            var result = await _users.UpdateAsync(user);
            if (!result.Succeeded)
                return ToValidationProblem(result);

            return Ok(ToResponse(user));
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            var result = await _users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return ToValidationProblem(result);

            var active = await _db.RefreshTokens
                .Where(t => t.UserId == user.Id && t.RevokedAt == null).ToListAsync();
            foreach (var token in active)
                token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            var result = await _users.DeleteAsync(user);
            if (!result.Succeeded)
                return ToValidationProblem(result);

            return NoContent();
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var sub = User.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out _) ? await _users.FindByIdAsync(sub!) : null;
        }

        private static UserProfileResponse ToResponse(User u) =>
        new(u.Id, u.Email ?? string.Empty, u.DisplayName, u.AvatarUrl, u.Bio, u.CreatedAt);

        private ActionResult ToValidationProblem(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem(ModelState);
        }
    }
}