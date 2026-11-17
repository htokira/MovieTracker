using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private const int MaxPage = 1000;
        private const int MaxPageSize = 50;
        private const int DefaultPageSize = 20;

        private readonly MovieTrackerContext _db;

        public NotificationsController(MovieTrackerContext db)
        {
            _db = db;
        }

        // Мої сповіщення, нові зверху
        [HttpGet("me")]
        public async Task<ActionResult<PagedResultDto<NotificationResponse>>> GetMine(
            int page = 1, int pageSize = DefaultPageSize, bool unreadOnly = false)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            page = Math.Clamp(page, 1, MaxPage);
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var query = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResultDto<NotificationResponse>
            {
                Page = page,
                TotalResults = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            });
        }

        // Детальний перегляд одного сповіщення
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NotificationResponse>> GetById(Guid id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var notification = await _db.Notifications.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            return notification is null ? NotFound() : Ok(ToResponse(notification));
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notification is null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var unread = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unread)
                notification.IsRead = true;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static NotificationResponse ToResponse(Notification n) =>
            new(n.Id, n.Type.ToString(), n.Text, n.ActorId, n.IsRead, n.CreatedAt);

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}
