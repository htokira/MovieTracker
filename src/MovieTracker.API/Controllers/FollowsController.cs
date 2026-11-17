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
        private const int MaxPage = 100;
        private const int MaxPageSize = 50;
        private const int DefaultPageSize = 20;

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

            var actorName = await _db.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync();

            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                ActorId = currentUserId,
                Type = NotificationType.NewFollower,
                Text = $"Новий підписник: {actorName}",
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Паралельний запит уже створив цю підписку (порушення PK), це не помилка.
            }

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

        // Стрічка активності: відгуки і зміни статусів людей, на яких я підписаний
        [HttpGet("me/feed")]
        [Authorize]
        public async Task<ActionResult<PagedResultDto<FeedItemResponse>>> GetFeed(
            int page = 1, int pageSize = DefaultPageSize)
        {
            if (!TryGetUserId(out var currentUserId)) return Unauthorized();

            page = Math.Clamp(page, 1, MaxPage);
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var followedIds = _db.UserFollows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId);

            var reviewsQuery = _db.Set<Review>().AsNoTracking()
                .Where(r => followedIds.Contains(r.UserId));
            var statusesQuery = _db.Set<UserMovieStatus>().AsNoTracking()
                .Where(s => followedIds.Contains(s.UserId));

            var totalResults = await reviewsQuery.CountAsync() + await statusesQuery.CountAsync();

            // З кожного джерела беремо стільки, скільки може знадобитись для цієї сторінки,
            // потім зливаємо в пам'яті, сортуємо і вирізаємо сторінку.
            var take = page * pageSize;

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.UpdatedAt)
                .Take(take)
                .Select(r => new FeedItemResponse(
                    "review", r.UserId, r.User.DisplayName, r.User.AvatarUrl,
                    r.MovieId, r.Movie.Title, r.Movie.PosterUrl,
                    r.Rating, r.Text, null, r.UpdatedAt))
                .ToListAsync();

            var statuses = await statusesQuery
                .OrderByDescending(s => s.UpdatedAt)
                .Take(take)
                .Select(s => new FeedItemResponse(
                    "status", s.UserId, s.User.DisplayName, s.User.AvatarUrl,
                    s.MovieId, s.Movie.Title, s.Movie.PosterUrl,
                    null, null, s.Status, s.UpdatedAt))
                .ToListAsync();

            var items = reviews.Concat(statuses)
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new PagedResultDto<FeedItemResponse>
            {
                Page = page,
                TotalResults = totalResults,
                TotalPages = (int)Math.Ceiling(totalResults / (double)pageSize),
                Items = items
            });
        }

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}
