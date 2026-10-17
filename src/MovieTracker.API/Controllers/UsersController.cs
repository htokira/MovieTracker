using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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