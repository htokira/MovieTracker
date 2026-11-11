using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.Linq.Expressions;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        private readonly MovieTrackerContext _db;
        private readonly IMovieSyncService _movieSync;

        public CollectionsController(MovieTrackerContext db, IMovieSyncService movieSync)
        {
            _db = db;
            _movieSync = movieSync;
        }

        private static readonly Expression<Func<Collection, CollectionSummaryResponse>> ToSummary = c =>
            new CollectionSummaryResponse(c.Id, c.UserId, c.User.DisplayName, c.Name, c.Description,
                c.IsPublic, c.Movies.Count, c.CreatedAt, c.UpdatedAt);

        // ---------- CRUD колекцій ----------

        [HttpPost("api/collections")]
        [Authorize]
        public async Task<ActionResult<CollectionSummaryResponse>> Create(CreateCollectionRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            if (!await _db.Users.AnyAsync(u => u.Id == userId)) return Unauthorized();

            var now = DateTime.UtcNow;
            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsPublic = request.IsPublic,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Collections.Add(collection);
            await _db.SaveChangesAsync();

            var response = await _db.Collections.AsNoTracking()
                .Where(c => c.Id == collection.Id).Select(ToSummary).SingleAsync();

            return CreatedAtAction(nameof(GetById), new { id = collection.Id }, response);
        }

        [HttpGet("api/collections/me")]
        [Authorize]
        public async Task<ActionResult<List<CollectionSummaryResponse>>> GetMine()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var items = await _db.Collections.AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(ToSummary)
                .ToListAsync();

            return Ok(items);
        }

        // Публічну колекцію бачать усі, приватну тільки власник.
        // Авторизація необов'язкова: якщо токен є, ми знаємо, хто питає.
        [HttpGet("api/collections/{id:guid}")]
        public async Task<ActionResult<CollectionDetailResponse>> GetById(Guid id)
        {
            TryGetUserId(out var currentUserId); // для анонімного буде Guid.Empty

            var result = await _db.Collections.AsNoTracking()
                .Where(c => c.Id == id && (c.IsPublic || c.UserId == currentUserId))
                .Select(c => new CollectionDetailResponse(
                    c.Id, c.UserId, c.User.DisplayName, c.Name, c.Description,
                    c.IsPublic, c.CreatedAt, c.UpdatedAt,
                    c.Movies
                        .OrderByDescending(cm => cm.AddedAt)
                        .Select(cm => new CollectionMovieResponse(
                            cm.MovieId, cm.Movie.Title, cm.Movie.PosterUrl,
                            cm.Movie.ReleaseYear, cm.Movie.Rating, cm.AddedAt))
                        .ToList()))
                .FirstOrDefaultAsync();

            return result is null ? NotFound() : Ok(result);
        }

        [HttpPut("api/collections/{id:guid}")]
        [Authorize]
        public async Task<ActionResult<CollectionSummaryResponse>> Update(Guid id, UpdateCollectionRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (collection is null) return NotFound();

            collection.Name = request.Name.Trim();
            collection.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            collection.IsPublic = request.IsPublic;
            collection.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var response = await _db.Collections.AsNoTracking()
                .Where(c => c.Id == id).Select(ToSummary).SingleAsync();
            return Ok(response);
        }

        [HttpDelete("api/collections/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (collection is null) return NotFound();

            _db.Collections.Remove(collection);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Фільми в колекції ----------

        [HttpPost("api/collections/{id:guid}/movies/{movieId:int:min(1)}")]
        [Authorize]
        public async Task<IActionResult> AddMovie(Guid id, int movieId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (collection is null) return NotFound();

            // Повторне додавання не помилка: просто нічого не робимо
            if (await _db.CollectionMovies.AnyAsync(cm => cm.CollectionId == id && cm.MovieId == movieId))
                return NoContent();

            if (!await _movieSync.EnsureMovieAsync(movieId))
                return NotFound(new { message = $"Movie with ID {movieId} not found on TMDB." });

            _db.CollectionMovies.Add(new CollectionMovie
            {
                CollectionId = id,
                MovieId = movieId,
                AddedAt = DateTime.UtcNow
            });
            collection.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("api/collections/{id:guid}/movies/{movieId:int:min(1)}")]
        [Authorize]
        public async Task<IActionResult> RemoveMovie(Guid id, int movieId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var collection = await _db.Collections.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (collection is null) return NotFound();

            var link = await _db.CollectionMovies
                .FirstOrDefaultAsync(cm => cm.CollectionId == id && cm.MovieId == movieId);
            if (link is null) return NotFound();

            _db.CollectionMovies.Remove(link);
            collection.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Колекції інших користувачів ----------

        // Тут тільки публічні, навіть якщо питає сам власник (свої бери через /me)
        [HttpGet("api/users/{userId:guid}/collections")]
        public async Task<ActionResult<List<CollectionSummaryResponse>>> GetUserPublicCollections(Guid userId)
        {
            var items = await _db.Collections.AsNoTracking()
                .Where(c => c.UserId == userId && c.IsPublic)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(ToSummary)
                .ToListAsync();

            return Ok(items);
        }

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}