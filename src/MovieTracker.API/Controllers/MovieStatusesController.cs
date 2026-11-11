using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.ComponentModel.DataAnnotations;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api")]
    public class MovieStatusesController : ControllerBase
    {
        private readonly MovieTrackerContext _db;
        private readonly IMovieStorageService _movieStorage;

        public MovieStatusesController(MovieTrackerContext db, IMovieStorageService movieStorage)
        {
            _db = db;
            _movieStorage = movieStorage;
        }

        [HttpGet("movies/{movieId:int:min(1)}/status")]
        public async Task<ActionResult<WatchStatusResponse>> GetStatus(int movieId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var status = await _db.UserMovieStatuses
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.MovieId == movieId)
                .Select(s => new WatchStatusResponse(s.MovieId, s.Status, s.UpdatedAt))
                .FirstOrDefaultAsync();

            return status is null ? NotFound() : Ok(status);
        }

        [HttpPut("movies/{movieId:int:min(1)}/status")]
        public async Task<ActionResult<WatchStatusResponse>> SetStatus(int movieId, SetWatchStatusRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            if (!Enum.IsDefined(request.Status))
            {
                ModelState.AddModelError(nameof(request.Status),
                    "Допустимі статуси: Watched, Planned, Dropped.");
                return ValidationProblem(ModelState);
            }

            if (!await _db.Users.AnyAsync(u => u.Id == userId)) return Unauthorized();

            if (!await _movieStorage.EnsureMovieExistsAsync(movieId))
                return NotFound(new { message = $"Movie with ID {movieId} not found on TMDB." });

            var now = DateTime.UtcNow;
            var entry = await _db.UserMovieStatuses
                .FirstOrDefaultAsync(s => s.UserId == userId && s.MovieId == movieId);

            if (entry is null)
            {
                entry = new UserMovieStatus
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MovieId = movieId,
                    CreatedAt = now
                };
                _db.UserMovieStatuses.Add(entry);
            }

            entry.Status = request.Status;
            entry.UpdatedAt = now;
            await _db.SaveChangesAsync();

            return Ok(new WatchStatusResponse(entry.MovieId, entry.Status, entry.UpdatedAt));
        }

        [HttpDelete("movies/{movieId:int:min(1)}/status")]
        public async Task<IActionResult> DeleteStatus(int movieId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var entry = await _db.UserMovieStatuses
                .FirstOrDefaultAsync(s => s.UserId == userId && s.MovieId == movieId);
            if (entry is null) return NotFound();

            _db.UserMovieStatuses.Remove(entry);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("users/me/movies")]
        public async Task<ActionResult<UserMoviesResponse>> GetMyMovies(
            [FromQuery] WatchStatus? status,
            [FromQuery, Range(1, 10000)] int page = 1,
            [FromQuery, Range(1, 50)] int pageSize = 20)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            if (status is not null && !Enum.IsDefined(status.Value))
            {
                ModelState.AddModelError(nameof(status),
                    "Допустимі статуси: Watched, Planned, Dropped.");
                return ValidationProblem(ModelState);
            }

            var query = _db.UserMovieStatuses.AsNoTracking().Where(s => s.UserId == userId);
            if (status is not null)
                query = query.Where(s => s.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.UpdatedAt)
                .ThenBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new WatchStatusItemResponse(
                    s.MovieId, s.Movie.Title, s.Movie.PosterUrl, s.Movie.ReleaseYear,
                    s.Status, s.UpdatedAt))
                .ToListAsync();

            return Ok(new UserMoviesResponse(page, pageSize, total, items));
        }

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}