using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class FollowsController : ControllerBase
    {
        private readonly MovieTrackerContext _db;

        public FollowsController(MovieTrackerContext db)
        {
            _db = db;
        }

        // Підписатися. Повторна підписка не помилка (як і в колекціях).
        [HttpPost("{userId:guid}/follow")]
        [Authorize]
        public async Task<IActionResult> Follow(Guid userId)
        {
            if (!TryGetUserId(out var currentUserId)) return Unauthorized();

            if (userId == currentUserId)
                return BadRequest(new { message = "You cannot follow yourself." });

            if (!await _db.Users.AnyAsync(u => u.Id == userId))
                return NotFound();

            if (await _db.UserFollows.AnyAsync(f =>
                    f.FollowerId == currentUserId && f.FollowingId == userId))
                return NoContent();

            _db.UserFollows.Add(new UserFollow
            {
                FollowerId = currentUserId,
                FollowingId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{userId:guid}/follow")]
        [Authorize]
        public async Task<IActionResult> Unfollow(Guid userId)
        {
            if (!TryGetUserId(out var currentUserId)) return Unauthorized();

            var follow = await _db.UserFollows.FirstOrDefaultAsync(f =>
                f.FollowerId == currentUserId && f.FollowingId == userId);
            if (follow is null) return NotFound();

            _db.UserFollows.Remove(follow);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Хто підписаний на користувача
        [HttpGet("{userId:guid}/followers")]
        public async Task<ActionResult<List<FollowUserResponse>>> GetFollowers(Guid userId)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == userId)) return NotFound();

            var items = await _db.UserFollows.AsNoTracking()
                .Where(f => f.FollowingId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FollowUserResponse(
                    f.FollowerId, f.Follower.DisplayName, f.Follower.AvatarUrl, f.CreatedAt))
                .ToListAsync();

            return Ok(items);
        }

        // На кого підписаний користувач
        [HttpGet("{userId:guid}/following")]
        public async Task<ActionResult<List<FollowUserResponse>>> GetFollowing(Guid userId)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == userId)) return NotFound();

            var items = await _db.UserFollows.AsNoTracking()
                .Where(f => f.FollowerId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FollowUserResponse(
                    f.FollowingId, f.Following.DisplayName, f.Following.AvatarUrl, f.CreatedAt))
                .ToListAsync();

            return Ok(items);
        }

        // Лічильники і чи підписаний поточний користувач (авторизація необов'язкова)
        [HttpGet("{userId:guid}/follow-stats")]
        public async Task<ActionResult<FollowStatsResponse>> GetStats(Guid userId)
        {
            TryGetUserId(out var currentUserId); // для анонімного буде Guid.Empty

            if (!await _db.Users.AnyAsync(u => u.Id == userId)) return NotFound();

            var followers = await _db.UserFollows.CountAsync(f => f.FollowingId == userId);
            var following = await _db.UserFollows.CountAsync(f => f.FollowerId == userId);
            var isFollowedByMe = currentUserId != Guid.Empty &&
                await _db.UserFollows.AnyAsync(f =>
                    f.FollowerId == currentUserId && f.FollowingId == userId);

            return Ok(new FollowStatsResponse(followers, following, isFollowedByMe));
        }

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}